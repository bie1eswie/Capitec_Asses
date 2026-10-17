using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bams.Application;
using Bams.Application.Abstractions.Communications;
using Bams.Application.Abstractions.Security;
using Bams.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Bams.Domain.Tests;

public sealed class CreateAppointmentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesBookingAndPersistsRelatedRecords()
    {
        var setup = new BookingSetup();

        var result = await setup.UseCase.ExecuteAsync(setup.CreateRequest(), "booking-key", CancellationToken.None);

        Assert.False(result.IsReplay);
        Assert.Single(setup.Customers.Items);
        Assert.Single(setup.Appointments.Items);
        Assert.Equal(setup.Customers.Items[0].Id, setup.Appointments.Items[0].CustomerId);
        Assert.Equal("account-1", setup.Appointments.Items[0].OwnerUserId);
        Assert.Single(setup.BookingRequests.Items);
        Assert.Single(setup.Outbox.Items);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidInputBeforeOpeningTransaction()
    {
        var setup = new BookingSetup();
        var request = new CreateAppointmentCommand
        {
            BranchId = setup.BranchId,
            ServiceId = setup.ServiceId,
            StartsAt = DateTimeOffset.Now.AddDays(1)
        };

        await Assert.ThrowsAsync<BookingException>(() => setup.UseCase.ExecuteAsync(
            request,
            "booking-key",
            CancellationToken.None));

        Assert.False(setup.UnitOfWork.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExistingBookingForDuplicateRequest()
    {
        var setup = new BookingSetup();
        var appointment = setup.CreateSavedAppointment();
        setup.Appointments.SavedAppointment = appointment;
        setup.BookingRequests.ExistingRequest = new BookingRequest
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = "booking-key",
            RequestHash = setup.Idempotency.ComputeHash(setup.CreateRequest(), "account-1"),
            AppointmentId = appointment.Id,
            OwnerUserId = "account-1",
            Operation = "create-appointment",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await setup.UseCase.ExecuteAsync(setup.CreateRequest(), "booking-key", CancellationToken.None);

        Assert.True(result.IsReplay);
        Assert.Equal(appointment.Id, result.Id);
        Assert.Empty(setup.Appointments.Items);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnavailableCapacityWithoutStagingWrites()
    {
        var setup = new BookingSetup();
        setup.Appointments.OverlappingAppointments =
        [
            new Appointment(
                Guid.NewGuid(),
                "BAMS-EXISTING",
                Guid.NewGuid(),
                setup.BranchId,
                setup.ServiceId,
                setup.CreateRequest().StartsAt,
                setup.CreateRequest().StartsAt.AddMinutes(15))
        ];

        await Assert.ThrowsAsync<BookingException>(() => setup.UseCase.ExecuteAsync(
            setup.CreateRequest(),
            "booking-key",
            CancellationToken.None));

        Assert.Single(setup.Customers.Items);
        Assert.Empty(setup.Appointments.Items);
        Assert.Empty(setup.Outbox.Items);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCancellationTokenToRepositoryCalls()
    {
        var setup = new BookingSetup();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;

        await setup.UseCase.ExecuteAsync(setup.CreateRequest(), "booking-key", token);

        Assert.Equal(token, setup.BookingRequests.LastToken);
        Assert.Equal(token, setup.Appointments.LastOverlapToken);
        Assert.Equal(token, setup.UnitOfWork.LastToken);
    }

    private sealed class BookingSetup
    {
        public Guid BranchId { get; } = Guid.NewGuid();
        public Guid ServiceId { get; } = Guid.NewGuid();
        public FakeUnitOfWork UnitOfWork { get; } = new();
        public FakeAppointmentRepository Appointments { get; } = new();
        public FakeBookingRequestRepository BookingRequests { get; } = new();
        public FakeCustomerRepository Customers { get; } = new();
        public FakeOutboxRepository Outbox { get; } = new();
        public FakeCurrentUserService CurrentUser { get; } = new("account-1");
        public IdempotencyProvider Idempotency { get; } = new();
        private readonly FakeBranchRepository branches;
        private readonly FakeServiceRepository services;
        private readonly FakeScheduleRepository schedules = new();

        public BookingSetup()
        {
            Customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                IdentityUserId = CurrentUser.UserId,
                FirstName = "Test",
                LastName = "Customer",
                Email = "customer@example.test"
            });
            branches = new FakeBranchRepository(BranchId, ServiceId);
            services = new FakeServiceRepository(ServiceId);
            var idempotencyService = new IdempotencyService(BookingRequests, Appointments, Idempotency);
            var availability = new AvailabilityService(Appointments, schedules);
            UseCase = new CreateAppointmentUseCase(
                Appointments,
                UnitOfWork,
                branches,
                services,
                BookingRequests,
                Customers,
                CurrentUser,
                Outbox,
                new BookingValidator(),
                availability,
                idempotencyService,
                new AppointmentFactory(),
                new OutboxFactory(Outbox),
                NullLogger<CreateAppointmentUseCase>.Instance);
        }

        public CreateAppointmentUseCase UseCase { get; }

        public CreateAppointmentCommand CreateRequest()
        {
            var startsAt = DateTimeOffset.UtcNow.AddDays(2);
            startsAt = new DateTimeOffset(startsAt.Year, startsAt.Month, startsAt.Day, 10, 0, 0, TimeSpan.Zero);
            return new CreateAppointmentCommand
            {
                BranchId = BranchId,
                ServiceId = ServiceId,
                StartsAt = startsAt
            };
        }

        public Appointment CreateSavedAppointment()
        {
            return new Appointment(
                Guid.NewGuid(),
                "BAMS-SAVED",
                Guid.NewGuid(),
                BranchId,
                ServiceId,
                CreateRequest().StartsAt,
                CreateRequest().StartsAt.AddMinutes(15));
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool WasCalled { get; private set; }
        public CancellationToken LastToken { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<T> ExecuteLockedAsync<T>(Guid branchId, Guid serviceId, Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken, string? operationLockKey = null)
        {
            WasCalled = true;
            LastToken = cancellationToken;
            return operation(cancellationToken);
        }

    }

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        public List<Appointment> Items { get; } = [];
        public Appointment? SavedAppointment { get; set; }
        public IReadOnlyList<Appointment> OverlappingAppointments { get; set; } = [];
        public CancellationToken LastOverlapToken { get; private set; }

        public void Add(Appointment appointment)
        {
            Items.Add(appointment);
        }

        public Task<Appointment?> FindAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(SavedAppointment?.Id == id ? SavedAppointment : null);
        }

        public Task<Appointment?> FindOwnedAsync(Guid id, string identityUserId, CancellationToken cancellationToken) =>
            Task.FromResult(SavedAppointment?.Id == id && SavedAppointment.Customer.IdentityUserId == identityUserId ? SavedAppointment : null);

        public Task<IReadOnlyList<Appointment>> ListOwnedAsync(string identityUserId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Appointment>>(Items.Where(appointment => appointment.Customer.IdentityUserId == identityUserId).ToArray());

        public Task<IReadOnlyList<Appointment>> OverlappingAsync(Guid branchId, Guid serviceId, DateTimeOffset from, DateTimeOffset to, Guid? excludingId, CancellationToken cancellationToken)
        {
            LastOverlapToken = cancellationToken;
            return Task.FromResult(OverlappingAppointments);
        }
    }

    private sealed class FakeBranchRepository(Guid branchId, Guid serviceId) : IBranchRepository
    {
        private readonly Guid configuredBranchId = branchId;
        private readonly Guid configuredServiceId = serviceId;
        private readonly Branch branch = CreateBranch(branchId);

        private static Branch CreateBranch(Guid id)
        {
            return new Branch
            {
                Id = id,
                Code = "BRANCH",
                Name = "Test Branch",
                City = "Cape Town",
                Address = "1 Main Street",
                TimeZone = "UTC",
                Status = BranchStatus.Active
            };
        }

        public Task<Branch?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Branch?>(id == configuredBranchId ? branch : null);
        public Task<BranchService?> FindBranchServiceAsync(Guid requestedBranchId, Guid requestedServiceId, CancellationToken cancellationToken) => Task.FromResult<BranchService?>(requestedBranchId == configuredBranchId && requestedServiceId == configuredServiceId ? new BranchService { BranchId = requestedBranchId, ServiceId = requestedServiceId, Capacity = 1, IsEnabled = true } : null);
        public Task<IReadOnlyList<Branch>> ListActiveAsync(string? search, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Branch>>([]);
        public Task<IReadOnlyList<Service>> ListActiveServicesAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Service>>([]);
    }

    private sealed class FakeServiceRepository(Guid serviceId) : IServiceRepository
    {
        public Task<Service?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Service?>(id == serviceId ? new Service { Id = id, Code = "SERVICE", Name = "Test Service", DurationMinutes = 15, IsActive = true } : null);
        public Task<IReadOnlyList<Service>> ListActiveAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Service>>([]);
    }

    private sealed class FakeBookingRequestRepository : IBookingRequestRepository
    {
        public List<BookingRequest> Items { get; } = [];
        public BookingRequest? ExistingRequest { get; set; }
        public CancellationToken LastToken { get; private set; }

        public Task<BookingRequest?> FindByIdempotencyKeyAsync(string key, string ownerUserId, string operation, CancellationToken cancellationToken)
        {
            LastToken = cancellationToken;
            return Task.FromResult(ExistingRequest?.IdempotencyKey == key && ExistingRequest.OwnerUserId == ownerUserId && ExistingRequest.Operation == operation ? ExistingRequest : null);
        }

        public void Add(BookingRequest request)
        {
            Items.Add(request);
        }
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public List<Customer> Items { get; } = [];
        public void Add(Customer customer) => Items.Add(customer);

        public Task<Customer?> FindByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(customer => customer.IdentityUserId == identityUserId));
    }

    private sealed class FakeCurrentUserService(string? userId) : ICurrentUserService
    {
        public string? UserId { get; } = userId;
    }

    private sealed class FakeOutboxRepository :
        IOutboxRepository,
        Bams.Application.Abstractions.Communications.IEventPublisher,
        Bams.Application.Abstractions.Communications.IMessageSerializer
    {
        public List<OutboxEvent> Items { get; } = [];

        public void Add(OutboxEvent item) => Items.Add(item);

        public Task PublishAsync(
            Bams.Application.Abstractions.Communications.EventEnvelope envelope,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Add(new OutboxEvent
            {
                Id = envelope.EventId,
                EventType = envelope.EventName,
                Version = envelope.Version,
                CorrelationId = envelope.CorrelationId,
                PayloadJson = envelope.Payload,
                OccurredAt = envelope.OccurredAt
            });
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxEvent>> ClaimPendingAsync(
            int limit,
            DateTimeOffset now,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken,
            int maximumAttempts = 10) =>
            Task.FromResult<IReadOnlyList<OutboxEvent>>([]);

        public Task MarkPublishedAsync(
            Guid eventId,
            DateTimeOffset publishedAt,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task MarkFailedAsync(
            Guid eventId,
            DateTimeOffset nextAttemptAt,
            string error,
            bool deadLetter,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> ReplayDeadLetterAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public string Serialize<TEvent>(TEvent eventData) =>
            System.Text.Json.JsonSerializer.Serialize(eventData);

        public TEvent Deserialize<TEvent>(string payload) =>
            System.Text.Json.JsonSerializer.Deserialize<TEvent>(payload)!;
    }

    private sealed class FakeScheduleRepository : IScheduleRepository
    {
        public Task<bool> IsOpenAsync(Guid branchId, DateTimeOffset start, DateTimeOffset end, string timeZone, CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
