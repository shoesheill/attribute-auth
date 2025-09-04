-- Seed regions/locations
INSERT INTO region (id, code, name) VALUES
    (uuid_generate_v4(), 'NP', 'Nepal')
ON CONFLICT DO NOTHING;

-- pick the Nepal region id
WITH np AS (SELECT id FROM region WHERE code='NP' LIMIT 1)
INSERT INTO location (code, name, region_id) VALUES
    ('KTM-1', 'Kathmandu Center', (SELECT id FROM np)),
    ('PKR-1', 'Pokhara Lakeside', (SELECT id FROM np))
ON CONFLICT DO NOTHING;

-- Seed roles with hierarchy
-- NepalHead > RegionalHead > LocalHead
INSERT INTO role (name, description, parent_role_id)
VALUES ('NepalHead', 'Superuser for all regions', NULL)
ON CONFLICT DO NOTHING;

WITH nh AS (SELECT id FROM role WHERE name='NepalHead')
INSERT INTO role (name, description, parent_role_id)
VALUES ('RegionalHead', 'Manages a region', (SELECT id FROM nh))
ON CONFLICT DO NOTHING;

WITH rh AS (SELECT id FROM role WHERE name='RegionalHead')
INSERT INTO role (name, description, parent_role_id)
VALUES ('LocalHead', 'Manages a location', (SELECT id FROM rh))
ON CONFLICT DO NOTHING;

-- Permissions
INSERT INTO permission (resource, action) VALUES
    ('resource', 'read'),
    ('resource', 'upsert'),
    ('resource', 'delete')
ON CONFLICT DO NOTHING;

-- Assign permissions
-- LocalHead: read/upsert own, read all at location (model will enforce "own" for upsert/delete)
-- For RBAC, give LocalHead all three actions on "resource" (ABAC handler will still restrict write to owner)
WITH lh AS (SELECT id FROM role WHERE name='LocalHead'),
     rh AS (SELECT id FROM role WHERE name='RegionalHead'),
     nh AS (SELECT id FROM role WHERE name='NepalHead')
INSERT INTO role_permission (role_id, permission_id)
SELECT lh.id, p.id FROM lh, permission p WHERE p.resource='resource'
ON CONFLICT DO NOTHING;

-- LocalHead
WITH lh AS (SELECT id FROM role WHERE name = 'LocalHead')
INSERT INTO role_permission (role_id, permission_id)
SELECT lh.id, p.id
FROM lh
         CROSS JOIN permission p
WHERE p.resource = 'resource'
ON CONFLICT DO NOTHING;

-- RegionalHead
WITH rh AS (SELECT id FROM role WHERE name = 'RegionalHead')
INSERT INTO role_permission (role_id, permission_id)
SELECT rh.id, p.id
FROM rh
         CROSS JOIN permission p
WHERE p.resource = 'resource'
ON CONFLICT DO NOTHING;

-- NepalHead  (fixed typo ap.id -> p.id)
WITH nh AS (SELECT id FROM role WHERE name = 'NepalHead')
INSERT INTO role_permission (role_id, permission_id)
SELECT nh.id, p.id
FROM nh
         CROSS JOIN permission p
WHERE p.resource = 'resource'
ON CONFLICT DO NOTHING;


-- Users (passwords are plain here; in app we will hash)
INSERT INTO app_user (username, password_hash, display_name)
VALUES ('nepal_head', 'pass', 'Nepal Head'),
       ('regional_head', 'pass', 'Regional Head'),
       ('local_head_ktm', 'pass', 'Local Head KTM'),
       ('local_member_a', 'pass', 'Local Member A'),
       ('local_member_b', 'pass', 'Local Member B')
ON CONFLICT DO NOTHING;

-- Scopes
WITH np AS (SELECT id FROM region WHERE code='NP'),
     ktm AS (SELECT id FROM location WHERE code='KTM-1'),
     pkr AS (SELECT id FROM location WHERE code='PKR-1'),
     nh AS (SELECT id FROM role WHERE name='NepalHead'),
     rh AS (SELECT id FROM role WHERE name='RegionalHead'),
     lh AS (SELECT id FROM role WHERE name='LocalHead'),
     u1 AS (SELECT id FROM app_user WHERE username='nepal_head'),
     u2 AS (SELECT id FROM app_user WHERE username='regional_head'),
     u3 AS (SELECT id FROM app_user WHERE username='local_head_ktm')
INSERT INTO user_role_scope (user_id, role_id, scope)
SELECT (SELECT id FROM u1), (SELECT id FROM nh), 'global'
ON CONFLICT DO NOTHING;

-- NepalHead: global scope
INSERT INTO user_role_scope (user_id, role_id, scope)
SELECT u.id, r.id, 'global'
FROM app_user u
         JOIN role r ON r.name = 'NepalHead'
WHERE u.username = 'nepal_head'
  AND NOT EXISTS (
    SELECT 1 FROM user_role_scope urs
    WHERE urs.user_id = u.id AND urs.role_id = r.id AND urs.scope = 'global'
);

-- RegionalHead: region = NP
INSERT INTO user_role_scope (user_id, role_id, scope, region_id)
SELECT u.id, r.id, 'region', reg.id
FROM app_user u
         JOIN role r ON r.name = 'RegionalHead'
         JOIN region reg ON reg.code = 'NP'
WHERE u.username = 'regional_head'
  AND NOT EXISTS (
    SELECT 1 FROM user_role_scope urs
    WHERE urs.user_id = u.id AND urs.role_id = r.id AND urs.scope = 'region' AND urs.region_id = reg.id
);

-- LocalHead: location = KTM-1
INSERT INTO user_role_scope (user_id, role_id, scope, location_id)
SELECT u.id, r.id, 'location', loc.id
FROM app_user u
         JOIN role r ON r.name = 'LocalHead'
         JOIN location loc ON loc.code = 'KTM-1'
WHERE u.username = 'local_head_ktm'
  AND NOT EXISTS (
    SELECT 1 FROM user_role_scope urs
    WHERE urs.user_id = u.id AND urs.role_id = r.id AND urs.scope = 'location' AND urs.location_id = loc.id
);
-- Sample resources (owned by local members, but in specific locations)
WITH np AS (SELECT id FROM region WHERE code='NP'),
     ktm AS (SELECT id FROM location WHERE code='KTM-1'),
     pkr AS (SELECT id FROM location WHERE code='PKR-1'),
     lmA AS (SELECT id FROM app_user WHERE username='local_member_a'),
     lmB AS (SELECT id FROM app_user WHERE username='local_member_b')
INSERT INTO app_resource (title, owner_id, region_id, location_id)
VALUES ('KTM Report - A', (SELECT id FROM lmA), (SELECT id FROM np), (SELECT id FROM ktm)),
       ('KTM Report - B', (SELECT id FROM lmB), (SELECT id FROM np), (SELECT id FROM ktm)),
       ('PKR Report - A', (SELECT id FROM lmA), (SELECT id FROM np), (SELECT id FROM pkr))
ON CONFLICT DO NOTHING;
