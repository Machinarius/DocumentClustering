namespace DocumentClusteringCore.Orchestration.MPI {
  public class MPIRemoteWorkerNode : IWorkerNode {
    public int Id { get; }

    public MPIRemoteWorkerNode(int id) {
      Id = id;
    }
  }
}
