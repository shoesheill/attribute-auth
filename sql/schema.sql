-- PostgreSQL schema for dynamic hierarchical authorization with location/region scoping

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users
CREATE TABLE IF NOT EXISTS app_user (
                                        id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    display_name TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
    );

-- Regions and Locations (hierarchical geo)
CREATE TABLE IF NOT EXISTS region (
                                      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code TEXT UNIQUE NOT NULL,
    name TEXT NOT NULL
    );

CREATE TABLE IF NOT EXISTS location (
                                        id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code TEXT UNIQUE NOT NULL,
    name TEXT NOT NULL,
    region_id UUID NOT NULL REFERENCES region(id) ON DELETE CASCADE
    );

-- Roles are hierarchical (a role can inherit from a parent)
CREATE TABLE IF NOT EXISTS role (
                                    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT UNIQUE NOT NULL,          -- e.g., NepalHead, RegionalHead, LocalHead
    description TEXT,
    parent_role_id UUID NULL REFERENCES role(id) ON DELETE SET NULL
    );

-- Permissions model (resource + action) => RBAC
CREATE TABLE IF NOT EXISTS permission (
                                          id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    resource TEXT NOT NULL,             -- e.g., "resource", "ticket", "user"
    action TEXT NOT NULL,               -- e.g., "read", "upsert", "delete"
    UNIQUE(resource, action)
    );

-- Role to permission mapping
CREATE TABLE IF NOT EXISTS role_permission (
                                               role_id UUID NOT NULL REFERENCES role(id) ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES permission(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
    );

-- User to role assignment with optional scope (global/region/location)
-- Enum type
-- Table
CREATE TABLE IF NOT EXISTS user_role_scope (
                                               id BIGSERIAL PRIMARY KEY,  -- simple surrogate key
                                               user_id     UUID NOT NULL REFERENCES app_user(id) ON DELETE CASCADE,
    role_id     UUID NOT NULL REFERENCES role(id) ON DELETE CASCADE,
    scope       TEXT NOT NULL CHECK (scope IN ('global','region','location')),
    region_id   UUID NULL REFERENCES region(id) ON DELETE CASCADE,
    location_id UUID NULL REFERENCES location(id) ON DELETE CASCADE,

    -- Valid combinations
    CHECK (
(scope = 'global'   AND region_id IS NULL AND location_id IS NULL) OR
(scope = 'region'   AND region_id IS NOT NULL AND location_id IS NULL) OR
(scope = 'location' AND location_id IS NOT NULL)
    )
    );

-- Uniqueness per scope (no COALESCE needed)
CREATE UNIQUE INDEX IF NOT EXISTS ux_user_role_scope_global
    ON user_role_scope (user_id, role_id, scope)
    WHERE scope = 'global';

CREATE UNIQUE INDEX IF NOT EXISTS ux_user_role_scope_region
    ON user_role_scope (user_id, role_id, scope, region_id)
    WHERE scope = 'region';

CREATE UNIQUE INDEX IF NOT EXISTS ux_user_role_scope_location
    ON user_role_scope (user_id, role_id, scope, location_id)
    WHERE scope = 'location';


-- Resource example table (owned by a user, belongs to a location/region)
CREATE TABLE IF NOT EXISTS app_resource (
                                            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title TEXT NOT NULL,
    owner_id UUID NOT NULL REFERENCES app_user(id) ON DELETE CASCADE,
    region_id UUID NOT NULL REFERENCES region(id) ON DELETE RESTRICT,
    location_id UUID NOT NULL REFERENCES location(id) ON DELETE RESTRICT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
    );

-- Helper view to expand role inheritance (naive recursive CTE)
-- Returns all (role_id, inherited_permission_id)
CREATE OR REPLACE VIEW v_role_effective_permissions AS
WITH RECURSIVE role_tree AS (
    SELECT r.id AS role_id, r.parent_role_id
    FROM role r
    UNION ALL
    SELECT r2.id, r2.parent_role_id
    FROM role r2
             JOIN role_tree rt ON r2.parent_role_id = rt.role_id
)
SELECT DISTINCT r.id AS role_id, rp.permission_id
FROM role r
         LEFT JOIN role_permission rp ON rp.role_id = r.id
UNION
SELECT DISTINCT rt.parent_role_id AS role_id, rp.permission_id
FROM role_tree rt
         JOIN role_permission rp ON rp.role_id = rt.role_id
WHERE rt.parent_role_id IS NOT NULL;

-- Effective permissions for a user considering all roles
CREATE OR REPLACE VIEW v_user_effective_permissions AS
SELECT urs.user_id, p.resource, p.action
FROM user_role_scope urs
         JOIN v_role_effective_permissions vrep ON vrep.role_id = urs.role_id
         JOIN permission p ON p.id = vrep.permission_id
GROUP BY urs.user_id, p.resource, p.action;
