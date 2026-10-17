using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bams.Api;
using Bams.Application;
using Bams.Application.Abstractions.Communications;
using Bams.Application.Abstractions.Security;
using Bams.Domain;
using Bams.Infrastructure;
using Bams.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Bams.Integration.Tests;

public sealed class OutboxPostgreSqlIntegrationTests : IAsyncLifetime
{
    private PostgreSqlTestDatabase database = null!;

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("BAMS_TEST_POSTGRES")!;
        database = new PostgreSqlTestDatabase(connectionString);
        await database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await database.DisposeAsync();
    }

    [PostgreSqlFact]
    public async Task BookingRecordsAndOutboxCommitTogether()
    {
        var branchId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        await SeedCapacityAsync(branchId, serviceId);

        BookingResultDto result;
        await SeedIdentityCustomerAsync("account-1", "booking@example.test");
        await using (var context = database.CreateContext())
        {
            var service = CreateAppointmentService(context, "account-1");
            var request = new CreateAppointmentCommand
            {
                BranchId = branchId,
                ServiceId = serviceId,
                StartsAt = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(2).AddHours(10).AddMinutes(15), TimeSpan.Zero)
            };
            result = await service.BookAsync(request, "booking-key", CancellationToken.None);

            Assert.False(result.IsReplay);

            var replay = await service.BookAsync(request, "booking-key", CancellationToken.None);
            Assert.Equal(result.Id, replay.Id);
            Assert.True(replay.IsReplay);
        }

        await using (var otherContext = database.CreateContext())
        {
            var otherCustomerService = CreateAppointmentService(otherContext, "account-2");
            await Assert.ThrowsAsync<BookingException>(() => otherCustomerService.BookAsync(
                new CreateAppointmentCommand
                {
                    BranchId = branchId,
                    ServiceId = serviceId,
                    StartsAt = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(2).AddHours(10).AddMinutes(15), TimeSpan.Zero)
                },
                "booking-key",
                CancellationToken.None));
        }

        await using var verification = database.CreateContext();
        Assert.Equal(1, await verification.Customers.CountAsync());
        Assert.Equal(1, await verification.Appointments.CountAsync());
        Assert.Equal(1, await verification.BookingRequests.CountAsync());
        Assert.Equal(1, await verification.OutboxEvents.CountAsync());

        var committedAppointment = await verification.Appointments.SingleAsync();
        var committedEvent = await verification.OutboxEvents.SingleAsync();
        Assert.Equal(result.Id, committedAppointment.Id);
        Assert.Equal("appointment.confirmed", committedEvent.EventType);
        Assert.Contains(result.Id.ToString(), committedEvent.PayloadJson);
    }

    [PostgreSqlFact]
    public async Task FocusedRepositoriesQueryAndShareUnitOfWorkTransaction()
    {
        var branchId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        await SeedCapacityAsync(branchId, serviceId);

        await using (var context = database.CreateContext())
        {
            var branches = new BranchRepository(context);
            var services = new ServiceRepository(context);
            var schedules = new ScheduleRepository(context);
            Assert.NotNull(await branches.FindActiveByIdAsync(branchId, CancellationToken.None));
            Assert.NotNull(await branches.FindBranchServiceAsync(branchId, serviceId, CancellationToken.None));
            Assert.NotNull(await services.FindActiveByIdAsync(serviceId, CancellationToken.None));

            var scheduleStart = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(2).AddHours(10), TimeSpan.Zero);
            Assert.True(await schedules.IsOpenAsync(branchId, scheduleStart, scheduleStart.AddMinutes(15), "UTC", CancellationToken.None));

            var unit = new UnitOfWork(context, NullLogger<UnitOfWork>.Instance);
            var customers = new CustomerRepository(context);
            var appointments = new AppointmentRepository(context);
            var requests = new BookingRequestRepository(context);
            var outbox = new OutboxRepository(context);
            await unit.ExecuteLockedAsync(branchId, serviceId, _ =>
            {
                StageBooking(customers, appointments, requests, outbox, branchId, serviceId, $"atomic-{Guid.NewGuid():N}");
                return Task.FromResult(true);
            }, CancellationToken.None);
        }

        await using var verification = database.CreateContext();
        Assert.Equal(1, await verification.Customers.CountAsync());
        Assert.Equal(1, await verification.Appointments.CountAsync());
        Assert.Equal(1, await verification.BookingRequests.CountAsync());
        Assert.Equal(1, await verification.OutboxEvents.CountAsync());
    }

    [PostgreSqlFact]
    public async Task CustomerQueriesOnlyReturnLinkedOwnedAppointments()
    {
        var branchId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var legacyCustomerId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var otherAppointmentId = Guid.NewGuid();
        var legacyAppointmentId = Guid.NewGuid();
        await SeedCapacityAsync(branchId, serviceId);

        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(CreateIdentityUser("account-1"), CreateIdentityUser("account-2"));
            context.Customers.AddRange(
                CreateCustomer(customerId, "account-1"),
                CreateCustomer(otherCustomerId, "account-2"),
                CreateCustomer(legacyCustomerId));
            context.Appointments.AddRange(
                CreateAppointment(appointmentId, customerId, branchId, serviceId, "account-1"),
                CreateAppointment(otherAppointmentId, otherCustomerId, branchId, serviceId, "account-2"),
                CreateAppointment(legacyAppointmentId, legacyCustomerId, branchId, serviceId));
            await context.SaveChangesAsync();
        }

        await using var accountContext = database.CreateContext();
        var accountService = CreateAppointmentService(accountContext, "account-1");
        Assert.NotNull(await accountService.GetAsync(appointmentId, CancellationToken.None));
        Assert.Null(await accountService.GetAsync(otherAppointmentId, CancellationToken.None));
        Assert.Null(await accountService.GetAsync(legacyAppointmentId, CancellationToken.None));
        Assert.Equal(1, (await accountService.ListMineAsync(CancellationToken.None)).Count);
    }

    [PostgreSqlFact]
    public async Task FailedTransactionRollsBackAppointmentAndOutbox()
    {
        var branchId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        await SeedCapacityAsync(branchId, serviceId);

        await using (var seed = database.CreateContext())
        {
            seed.BookingRequests.Add(new BookingRequest
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = "duplicate-key",
                Operation = "create-appointment",
                RequestHash = "existing-request",
                AppointmentId = Guid.NewGuid(),
                OwnerUserId = "test-owner",
                CreatedAt = DateTimeOffset.UtcNow
            });
            await seed.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var unit = new UnitOfWork(context, NullLogger<UnitOfWork>.Instance);
            var appointments = new AppointmentRepository(context);
            var customers = new CustomerRepository(context);
            var bookingRequests = new BookingRequestRepository(context);
            var outbox = new OutboxRepository(context);

            await Assert.ThrowsAsync<DbUpdateException>(() => unit.ExecuteLockedAsync(
                branchId,
                serviceId,
                _ => Task.FromResult(StageBooking(customers, appointments, bookingRequests, outbox, branchId, serviceId, "duplicate-key", "test-owner").Id),
                CancellationToken.None));
        }

        await using var verification = database.CreateContext();
        Assert.Equal(0, await verification.Customers.CountAsync());
        Assert.Equal(0, await verification.Appointments.CountAsync());
        Assert.Equal(1, await verification.BookingRequests.CountAsync());
        Assert.Equal(0, await verification.OutboxEvents.CountAsync());
    }

    [PostgreSqlFact]
    public async Task ReplayedNotificationConsumptionCreatesOneRecord()
    {
        var branchId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var outboxEvent = CreateOutboxEvent(appointmentId);
        await SeedCapacityAsync(branchId, serviceId);

        await using (var context = database.CreateContext())
        {
            var appointment = CreateAppointment(appointmentId, customerId, branchId, serviceId);
            context.Customers.Add(CreateCustomer(customerId));
            context.Appointments.Add(appointment);
            context.OutboxEvents.Add(outboxEvent);
            await context.SaveChangesAsync();
        }

        await using var firstContext = database.CreateContext();
        await using var secondContext = database.CreateContext();
        var notification = new AppointmentNotificationEventV1(
            appointmentId,
            "BAMS-TEST",
            "confirmed",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(15));
        var envelope = new EventEnvelope(
            outboxEvent.Id,
            outboxEvent.EventType,
            outboxEvent.Version,
            outboxEvent.CorrelationId ?? outboxEvent.Id.ToString("N"),
            outboxEvent.OccurredAt,
            outboxEvent.PayloadJson);
        await new AppointmentNotificationEventHandler(firstContext).HandleAsync(
            notification,
            envelope,
            CancellationToken.None);
        await new AppointmentNotificationEventHandler(secondContext).HandleAsync(
            notification,
            envelope,
            CancellationToken.None);

        await using var verification = database.CreateContext();
        Assert.Equal(1, await verification.Notifications.CountAsync(x => x.OutboxEventId == outboxEvent.Id));
    }

    [PostgreSqlFact]
    public async Task FailedDispatchKeepsAppointmentAndOutboxForRetry()
    {
        var branchId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var outboxEvent = CreateOutboxEvent(appointmentId);
        await SeedCapacityAsync(branchId, serviceId);
        await using (var seed = database.CreateContext())
        {
            seed.Customers.Add(CreateCustomer(customerId));
            seed.Appointments.Add(CreateAppointment(appointmentId, customerId, branchId, serviceId));
            seed.OutboxEvents.Add(outboxEvent);
            await seed.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository = new OutboxRepository(context);
            var subscriber = new PostgreSqlEventSubscriber(
                repository,
                Microsoft.Extensions.Options.Options.Create(new MessagingOptions()));
            var dispatcher = new PostgreSqlEventWorkerDispatcher(
                subscriber,
                new EventDispatcher(new EventRegistry([]), []),
                NullLogger<PostgreSqlEventWorkerDispatcher>.Instance);
            var firstClaim = await subscriber.ReceiveAsync(CancellationToken.None)
                ?? throw new InvalidOperationException("Expected an outbox event claim.");

            await dispatcher.DispatchAsync(firstClaim, CancellationToken.None);
        }

        await using (var afterFailure = database.CreateContext())
        {
            var failedEvent = await afterFailure.OutboxEvents.SingleAsync(x => x.Id == outboxEvent.Id);
            Assert.Null(failedEvent.PublishedAt);
            Assert.Equal(1, failedEvent.Attempts);
            Assert.NotNull(failedEvent.NextAttemptAt);
            Assert.Null(failedEvent.LockedUntil);
            failedEvent.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(-1);
            await afterFailure.SaveChangesAsync();
        }

        await using (var retryContext = database.CreateContext())
        {
            var repository = new OutboxRepository(retryContext);
            var subscriber = new PostgreSqlEventSubscriber(
                repository,
                Microsoft.Extensions.Options.Options.Create(new MessagingOptions()));
            var eventDispatcher = new EventDispatcher(
                new EventRegistry(
                [
                    new EventDescriptor(
                        outboxEvent.EventType,
                        outboxEvent.Version,
                        typeof(AppointmentNotificationEventV1))
                ]),
                []);
            var dispatcher = new PostgreSqlEventWorkerDispatcher(
                subscriber,
                eventDispatcher,
                NullLogger<PostgreSqlEventWorkerDispatcher>.Instance);
            var retryClaim = await subscriber.ReceiveAsync(CancellationToken.None)
                ?? throw new InvalidOperationException("Expected the outbox event to be available for retry.");

            await dispatcher.DispatchAsync(retryClaim, CancellationToken.None);
        }

        await using var verification = database.CreateContext();
        var publishedEvent = await verification.OutboxEvents.SingleAsync(x => x.Id == outboxEvent.Id);
        Assert.True(await verification.Appointments.AnyAsync(x => x.Id == appointmentId));
        Assert.NotNull(publishedEvent.PublishedAt);
        Assert.Equal(1, publishedEvent.Attempts);
    }

    private async Task SeedCapacityAsync(Guid branchId, Guid serviceId)
    {
        await using var context = database.CreateContext();
        var branch = new Branch
        {
            Id = branchId,
            Code = $"B-{Guid.NewGuid():N}"[..24],
            Name = "Integration branch",
            City = "Cape Town",
            Address = "1 Main Street"
        };
        var service = new Service
        {
            Id = serviceId,
            Code = $"S-{Guid.NewGuid():N}"[..40],
            Name = "Integration service",
            DurationMinutes = 15
        };

        context.Branches.Add(branch);
        context.Services.Add(service);
        context.BranchServices.Add(new BranchService
        {
            BranchId = branchId,
            Branch = branch,
            ServiceId = serviceId,
            Service = service,
            Capacity = 1,
            IsEnabled = true
        });
        context.BranchSchedules.AddRange(Enumerable.Range(0, 7).Select(dayOfWeek => new BranchSchedule
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            DayOfWeek = dayOfWeek,
            OpensAt = TimeOnly.MinValue,
            ClosesAt = new TimeOnly(23, 59),
            IsActive = true
        }));
        await context.SaveChangesAsync();
    }

    private static OutboxEvent StageBooking(
        ICustomerRepository customers,
        IAppointmentRepository appointments,
        IBookingRequestRepository bookingRequests,
        IOutboxRepository outbox,
        Guid branchId,
        Guid serviceId,
        string idempotencyKey,
        string ownerUserId = "test-owner")
    {
        var customer = CreateCustomer(Guid.NewGuid());
        var appointment = CreateAppointment(Guid.NewGuid(), customer.Id, branchId, serviceId);
        var request = new BookingRequest
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            Operation = "create-appointment",
            RequestHash = Guid.NewGuid().ToString("N"),
            AppointmentId = appointment.Id,
            OwnerUserId = ownerUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var outboxEvent = CreateOutboxEvent(appointment.Id);

        customers.Add(customer);
        appointments.Add(appointment);
        bookingRequests.Add(request);
        outbox.Add(outboxEvent);
        return outboxEvent;
    }

    private static CreateAppointmentUseCase CreateAppointmentUseCase(BamsDbContext context, string identityUserId)
    {
        var appointments = new AppointmentRepository(context);
        var bookingRequests = new BookingRequestRepository(context);
        return new CreateAppointmentUseCase(
            appointments,
            new UnitOfWork(context, NullLogger<UnitOfWork>.Instance),
            new BranchRepository(context),
            new ServiceRepository(context),
            bookingRequests,
            new CustomerRepository(context),
            new TestCurrentUserService(identityUserId),
            new PostgreSqlEventPublisher(new OutboxRepository(context)),
            new BookingValidator(),
            new AvailabilityService(appointments, new ScheduleRepository(context)),
            new IdempotencyService(bookingRequests, appointments, new IdempotencyProvider()),
            new AppointmentFactory(),
            new OutboxFactory(new SystemTextJsonMessageSerializer()),
            NullLogger<CreateAppointmentUseCase>.Instance);
    }

    private static AppointmentService CreateAppointmentService(BamsDbContext context, string identityUserId)
    {
        var appointments = new AppointmentRepository(context);
        return new AppointmentService(
            appointments,
            new UnitOfWork(context, NullLogger<UnitOfWork>.Instance),
            new BranchRepository(context),
            new ServiceRepository(context),
            new ScheduleRepository(context),
            new PostgreSqlEventPublisher(new OutboxRepository(context)),
            new AppointmentTransitionRepository(context),
            new TestCurrentUserService(identityUserId),
            CreateAppointmentUseCase(context, identityUserId),
            NullLogger<AppointmentService>.Instance);
    }

    private async Task SeedIdentityCustomerAsync(string identityUserId, string email)
    {
        await using var context = database.CreateContext();
        context.Users.Add(CreateIdentityUser(identityUserId, email));
        context.Customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            IdentityUserId = identityUserId,
            FirstName = "Test",
            LastName = "Customer",
            Email = email
        });
        await context.SaveChangesAsync();
    }

    private static ApplicationUser CreateIdentityUser(string identityUserId, string? email = null) => new()
    {
        Id = identityUserId,
        UserName = email ?? $"{identityUserId}@example.test",
        NormalizedUserName = (email ?? $"{identityUserId}@example.test").ToUpperInvariant(),
        Email = email ?? $"{identityUserId}@example.test",
        NormalizedEmail = (email ?? $"{identityUserId}@example.test").ToUpperInvariant(),
        EmailConfirmed = true,
        SecurityStamp = Guid.NewGuid().ToString("N")
    };

    private static Customer CreateCustomer(Guid id, string? identityUserId = null) => new()
    {
        Id = id,
        IdentityUserId = identityUserId,
        FirstName = "Test",
        LastName = "Customer",
        Email = $"{id:N}@example.test"
    };

    private static Appointment CreateAppointment(Guid id, Guid customerId, Guid branchId, Guid serviceId, string? ownerUserId = null)
    {
        var startsAt = DateTimeOffset.UtcNow.AddDays(1);
        var appointment = new Appointment(
            id,
            $"BAMS-{Guid.NewGuid():N}"[..16],
            customerId,
            branchId,
            serviceId,
            startsAt,
            startsAt.AddMinutes(15))
        {
            OwnerUserId = ownerUserId
        };
        appointment.Confirm();
        return appointment;
    }

    private static OutboxEvent CreateOutboxEvent(Guid appointmentId) => new()
    {
        Id = Guid.NewGuid(),
        EventType = "appointment.confirmed",
        PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { appointmentId }),
        OccurredAt = DateTimeOffset.UtcNow
    };

    private sealed class TestCurrentUserService(string? userId) : ICurrentUserService
    {
        public string? UserId { get; } = userId;
    }
}
