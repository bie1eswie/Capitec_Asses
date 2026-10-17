BEGIN;
CREATE TABLE IF NOT EXISTS branch_schedules (
 id uuid PRIMARY KEY, branch_id uuid NOT NULL REFERENCES branches(id), day_of_week integer NOT NULL CHECK(day_of_week BETWEEN 0 AND 6),
 opens_at time NOT NULL, closes_at time NOT NULL, is_active boolean NOT NULL DEFAULT true,
 CONSTRAINT ck_schedule_times CHECK(closes_at>opens_at));
CREATE INDEX IF NOT EXISTS ix_branch_schedules ON branch_schedules(branch_id,day_of_week);
CREATE TABLE IF NOT EXISTS branch_closures (
 id uuid PRIMARY KEY,branch_id uuid NOT NULL REFERENCES branches(id),local_date date NOT NULL,reason text NOT NULL,
 CONSTRAINT uq_branch_closure UNIQUE(branch_id,local_date));
CREATE TABLE IF NOT EXISTS booking_requests (
 id uuid PRIMARY KEY,idempotency_key varchar(160) NOT NULL UNIQUE,request_hash varchar(64) NOT NULL,
 appointment_id uuid NOT NULL REFERENCES appointments(id),created_at timestamptz NOT NULL DEFAULT now());
-- Demo hours: Monday-Friday 09:00-17:00, Saturday 09:00-13:00, Sunday closed.
INSERT INTO branch_schedules(id,branch_id,day_of_week,opens_at,closes_at)
SELECT gen_random_uuid(),b.id,d.day, d.opens,d.closes FROM branches b CROSS JOIN
(VALUES(1,'09:00'::time,'17:00'::time),(2,'09:00'::time,'17:00'::time),
(3,'09:00'::time,'17:00'::time),(4,'09:00'::time,'17:00'::time),
(5,'09:00'::time,'17:00'::time),(6,'09:00'::time,'13:00'::time)) d(day,opens,closes)
WHERE NOT EXISTS(SELECT 1 FROM branch_schedules s WHERE s.branch_id=b.id AND s.day_of_week=d.day);
COMMIT;
