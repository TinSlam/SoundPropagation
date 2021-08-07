using System.Collections.Generic;

namespace SoundPropagation{
    public class VertexNode{
        public DiffractionSource source;
        public float gScore;
        public float fScore;
        public VertexNode parent;

        public VertexNode(DiffractionSource source, float gScore, float hScore, VertexNode parent){
            this.source = source;
            this.gScore = gScore;
            fScore = gScore + hScore;
            this.parent = parent;
        }

        public VertexNode(DiffractionSource source, float value){
            this.source = source;
            fScore = value;
            gScore = 0;
            parent = null;
        }
    }

    public class MinHeap{
        private List<VertexNode> elements;
        private int size;

        public MinHeap(){
            elements = new List<VertexNode>();
        }

        public void clear(){
            elements.Clear();
            size = 0;
        }

        private int GetLeftChildIndex(int elementIndex) => 2 * elementIndex + 1;
        private int GetRightChildIndex(int elementIndex) => 2 * elementIndex + 2;
        private int GetParentIndex(int elementIndex) => (elementIndex - 1) / 2;

        private bool HasLeftChild(int elementIndex) => GetLeftChildIndex(elementIndex) < size;
        private bool HasRightChild(int elementIndex) => GetRightChildIndex(elementIndex) < size;
        private bool IsRoot(int elementIndex) => elementIndex == 0;

        private VertexNode GetLeftChild(int elementIndex) => elements[GetLeftChildIndex(elementIndex)];
        private VertexNode GetRightChild(int elementIndex) => elements[GetRightChildIndex(elementIndex)];
        private VertexNode GetParent(int elementIndex) => elements[GetParentIndex(elementIndex)];

        private void Swap(int firstIndex, int secondIndex){
            var temp = elements[firstIndex];
            elements[firstIndex] = elements[secondIndex];
            elements[secondIndex] = temp;
        }

        public bool IsEmpty(){
            return size == 0;
        }

        public VertexNode Peek(){
            return elements[0];
        }

        public VertexNode Pop(){
            var result = elements[0];
            elements[0] = elements[size - 1];
            size--;

            elements.RemoveAt(size);

            ReCalculateDown();

            return result;
        }

        public void Add(VertexNode element){
            elements.Add(element);
            size++;

            ReCalculateUp();
        }

        private void ReCalculateDown(){
            int index = 0;
            while (HasLeftChild(index)){
                var smallerIndex = GetLeftChildIndex(index);
                if(HasRightChild(index) && GetRightChild(index).fScore < GetLeftChild(index).fScore)
                    smallerIndex = GetRightChildIndex(index);

                if(elements[smallerIndex].fScore >= elements[index].fScore)
                    break;

                Swap(smallerIndex, index);
                index = smallerIndex;
            }
        }

        private void ReCalculateUp(){
            var index = size - 1;
            while (!IsRoot(index) && elements[index].fScore < GetParent(index).fScore){
                var parentIndex = GetParentIndex(index);
                Swap(parentIndex, index);
                index = parentIndex;
            }
        }
    }
}