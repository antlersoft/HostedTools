namespace com.antlersoft.HostedTools.GraphFlow {
    /// <summary>
    /// A generalization of a Pipeline.
    /// A graph is a collection of configurable data-processing objects called Nodes connected by Edges
    /// which represent a the output of a Node being used as the input of another Node.
    /// <p>
    /// Each Node is associated with a NodeSpec.  A NodeSpec provides a "schema" which is an IHtValue object.  Elements in the schema
    /// which are simple string values are stream keys.  A NodeSpec combined with configuration information
    /// is a Node.
    /// <p>
    /// An edge in the graph is an association
    /// between a receiving node and a sending node identified by the stream key in the sending node's
    /// schema.
    /// <p>
    /// In a "proper" graph, all nodes have all their required inputs connected to edges.  When the
    /// user arranges a graph, they then "Run" it.  Running a graph first traverses it to confirm
    /// that it is a proper graph and to build a "Flow".  A "Flow" is an instantiation of the graph
    /// with a particular set of configurations for the nodes.
    /// <p>
    /// A Flow is executed starting at "leaf" nodes; nodes which do not have outgoing edges.  Each leaf
    /// "pulls" data from its input edges.  This may cause the nodes at the other end of the 
    /// </summary>
    public static class NamespaceDoc {

    }
}