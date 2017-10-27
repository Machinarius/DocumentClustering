namespace DocumentClusteringCore.Messaging.MPI.Internal {
  public static class MPIMessageTags {
    public static int SerializedMessagesTag = 0;

    public static int DocumentGeneratedTag = 1;
    public static int DocumentNormalizedTag = 2;
    public static int WorkAssignmentTag = 3;
    public static int AvailabilityChangesTag = 4;
  }
}
