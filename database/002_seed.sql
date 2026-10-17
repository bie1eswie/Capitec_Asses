INSERT INTO branches(id,code,name,city,address,time_zone,status) VALUES
('10000000-0000-0000-0000-000000000001','JHB-SND','Sandton Branch','Johannesburg','123 Rivonia Road, Sandton','Africa/Johannesburg','Active'),
('10000000-0000-0000-0000-000000000002','JHB-RSB','Rosebank Branch','Johannesburg','20 Oxford Road, Rosebank','Africa/Johannesburg','Active'),
('10000000-0000-0000-0000-000000000003','PTA-HAT','Hatfield Branch','Pretoria','101 Burnett Street, Hatfield','Africa/Johannesburg','Active') ON CONFLICT DO NOTHING;
INSERT INTO services(id,code,name,duration_minutes,is_active) VALUES
('20000000-0000-0000-0000-000000000001','ACCOUNT_OPEN','Account opening',45,true),
('20000000-0000-0000-0000-000000000002','CONSULT','General consultation',30,true),
('20000000-0000-0000-0000-000000000003','CARD','Card collection',15,true),
('20000000-0000-0000-0000-000000000004','DOCUMENTS','Document verification',20,true) ON CONFLICT DO NOTHING;
INSERT INTO branch_services(branch_id,service_id,capacity,is_enabled)
SELECT b.id,s.id,3,true FROM branches b CROSS JOIN services s ON CONFLICT DO NOTHING;
