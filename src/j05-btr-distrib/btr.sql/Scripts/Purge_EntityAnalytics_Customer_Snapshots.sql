-- Purge Customer Entity Analytics snapshots before identity-correct regeneration.
-- Run after deploying the CustomerId/EntityCode producer fix, then trigger
-- RefreshDashboardCustomerSnapshotWorker and historical Customer backfill replay.
-- Re-run Salesman, Item, and Supplier workers to refresh cross-entity L4 targets.

DELETE FROM BTRPD_EntityAnalytics_Radar WHERE EntityType = 'Customer';
DELETE FROM BTRPD_EntityAnalytics_Relationship
WHERE SourceEntityType = 'Customer' OR TargetEntityType = 'Customer';
DELETE FROM BTRPD_EntityAnalytics_Attention WHERE EntityType = 'Customer';
DELETE FROM BTRPD_EntityAnalytics_Ranking WHERE EntityType = 'Customer';
DELETE FROM BTRPD_EntityAnalytics_Monthly WHERE EntityType = 'Customer';
DELETE FROM BTRPD_EntityAnalytics_Current WHERE EntityType = 'Customer';
