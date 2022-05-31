using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain
{

    public class Functionality
    {
        internal Functionality(string name){
            Name = name;
        }
        public string Name { get; private set; }
    }

    public class DomainScope
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string ReadOnlyConnectionStringName { get; private set; }

        private IReadOnlyCollection<Functionality> functionalities;

        public IEnumerable<Functionality> Functionalities
        {
            get
            {
                if (functionalities == null)
                    return Enumerable.Empty<Functionality>();
                return functionalities;
            }
        }

        internal DomainScope(string name)
        {
            Name = name;
        }

        internal DomainScope(string name, IList<Functionality> functionalities)
            : this(name)
        {
            if (this.functionalities != null)
            {
                this.functionalities = new System.Collections.ObjectModel.ReadOnlyCollection<Functionality>(functionalities);
            }
            
        }

        internal DomainScope(string name, string readOnlyConnectionStringName)
            : this(name)
        {
            this.ReadOnlyConnectionStringName = readOnlyConnectionStringName;
        }

        internal DomainScope(string name, string description, IList<Functionality> functionalities)
            : this(name)
        {
            this.Description = description;
            if (functionalities != null)
            {
                this.functionalities = new System.Collections.ObjectModel.ReadOnlyCollection<Functionality>(functionalities);
            }

        }
    }

      
}
