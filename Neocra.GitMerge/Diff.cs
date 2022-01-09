using System.Collections.Generic;

namespace Neocra.GitMerge
{
    public class Diff<T> : Diff
    {
        public T Value { get; }

        public Diff(DiffMode mode, int indexOfChild, int moveIndexOfChild, T value) : base(mode, indexOfChild, moveIndexOfChild)
        {
            this.Value = value;
        }
    }
    
    public class Diff
    {
        public DiffMode Mode { get; }

        /// <summary>
        /// Gets index of the node in all children
        /// </summary>
        public int IndexOfChild { get; }
        
        /// <summary>
        /// Gets index of the move node in all children
        /// </summary>
        public int MoveIndexOfChild { get; set; }
        
        public Diff(DiffMode mode, int indexOfChild, int moveIndexOfChild)
        {
            this.Mode = mode;
            this.IndexOfChild = indexOfChild;
            this.MoveIndexOfChild = moveIndexOfChild;
        }

        public override string ToString()
        {
            var name = this.GetName();
            return this.Mode switch
            {
                DiffMode.Add => $"+ [{this.IndexOfChild}] ({name})",
                DiffMode.Delete => $"- [{this.IndexOfChild}] ({name})",
                DiffMode.Update => $"U [{this.IndexOfChild}] ({name})",
                DiffMode.Move => $"M [{this.IndexOfChild}=>{this.MoveIndexOfChild}] ({name})",
                _ => "Unknown"
            };
        }

        protected virtual string GetName()
        {
            return this.GetType().Name;
        }
    }
}