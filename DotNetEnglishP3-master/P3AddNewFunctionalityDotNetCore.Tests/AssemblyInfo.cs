// Integration tests share a single physical SQL Server database (P3Referential_Test)
// and recreate its schema per test. Disabling parallelization prevents test classes
// from clobbering each other's database state.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
