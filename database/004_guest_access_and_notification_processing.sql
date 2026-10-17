BEGIN;
CREATE TABLE IF NOT EXISTS guest_access_grants (
 id uuid PRIMARY KEY,
 appointment_id uuid NOT NULL REFERENCES appointments(id) ON DELETE CASCADE,
 token_hash varchar(64) NOT NULL UNIQUE,
 expires_at timestamptz NOT NULL,
 created_at timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_guest_grants_appointment ON guest_access_grants(appointment_id);
ALTER TABLE notifications ADD COLUMN IF NOT EXISTS outbox_event_id uuid REFERENCES outbox_events(id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_notification_event ON notifications(outbox_event_id);
COMMIT;
