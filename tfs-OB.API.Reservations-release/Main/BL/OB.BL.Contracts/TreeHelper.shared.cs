using System.Collections.ObjectModel;

namespace OB.Reservation.BL.Contracts
{
    /// <summary>
    /// Generic Node Class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Node<T>
    {
        // Private member-variables
        private T data;

        private NodeList<T> neighbors = null;

        public Node()
        {
        }

        public Node(T data)
            : this(data, null)
        {
        }

        public Node(T data, NodeList<T> neighbors)
        {
            this.data = data;
            this.neighbors = neighbors;
        }

        public T Value
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        public NodeList<T> Neighbors
        {
            get
            {
                return neighbors;
            }
            set
            {
                neighbors = value;
            }
        }
    }

    /// <summary>
    /// Generic Node List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeList<T> : Collection<Node<T>>
    {
        public NodeList()
            : base()
        {
        }

        public NodeList(int initialSize)
        {
            // Add the specified number of items
            for (int i = 0; i < initialSize; i++)
                base.Items.Add(default(Node<T>));
        }

        public Node<T> FindByValue(T value)
        {
            // search the list for the value
            foreach (Node<T> node in Items)
                if (node.Value.Equals(value))
                    return node;

            // if we reached here, we didn't find a matching node
            return null;
        }
    }
}