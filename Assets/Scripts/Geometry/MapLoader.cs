using System.Collections.Generic;
using UnityEngine;

namespace SoundPropagation{
    public class MapLoader{
        private static GameObject obstacleDataPrefab;

        public static List<Vector2[]> loadMap(string mapName){
            MapLoader mapLoader = new MapLoader("Assets/Resources/Maps/" + mapName);
            mapLoader.parseMap();
            return mapLoader.obstacles;
        }

        private string path;
        private List<LinkedList<Vector2>> obstacleVertices;
        private List<Vector2[]> obstacles = new List<Vector2[]>();

        private int width;
        private int height;

        private int[] data;
        private int[] grid;
        private bool[] visited;

        public MapLoader(string path){
            this.path = path;
        }

        public void parseMap(){
            string rawContent = FileManager.readFile(path);

            string[] content = rawContent.Split('\n');

            string error;

            if(!content[0].StartsWith("type octile")){
                error = "Map type should be \"octile\"!";
                goto label_error;
            }

            height = int.Parse(content[1].Substring(7));
            width = int.Parse(content[2].Substring(6));

            Vector2[] boundaryVertices = new Vector2[4];
            boundaryVertices[0] = new Vector2(0, 0);
            boundaryVertices[1] = new Vector2(width, 0);
            boundaryVertices[2] = new Vector2(width, height);
            boundaryVertices[3] = new Vector2(0, height);

            obstacles.Add(boundaryVertices);

            data = new int[(width + 2) * (height + 2)];

            for(int i = 0; i < width + 2; i++){
                data[i] = 0;
                data[(width + 2) * (height + 1) + i] = 0;
            }

            for(int i = 0; i < height + 2; i++){
                data[i * (width + 2)] = 0;
                data[i * (width + 2) + width + 1] = 0;
            }

            for(int i = 4; i < 4 + height; i++){
                int row = i - 4 + 1;

                for(int column = 0; column < width; column++){
                    int index = row * (width + 2) + column + 1;
                    switch(content[i][column]){
                        case '.': // Passable Terrain
                        case 'G': // Passable Terrain
                        case 'S': // Swamp: Passable from Regular Terrain
                        case 'W': // Water: Traversable, but not Passable from Terrain
                            data[index] = 0;
                            break;

                        case '@': // Out of Bounds
                        case 'O': // Out of Bounds
                        case 'T': // Tree: Impassable
                            data[index] = 1;
                            break;

                        default:
                            error = "Unknown character in map: " + content[i]/*[column]*/;
                            goto label_error;
                    }
                }
            }

            determineCells();

            createObstacles();

            foreach(LinkedList<Vector2> list in obstacleVertices){
                Vector2[] vertices = new Vector2[list.Count];
                LinkedListNode<Vector2> currentNode = list.First;
                int counter = 0;
                while(currentNode != null){
                    vertices[counter++] = currentNode.Value;
                    currentNode = currentNode.Next;
                }
                obstacles.Add(vertices);
            }

            return;

            label_error:
                Debug.LogError("Failed to load the map at location: " + path);
                Debug.LogError("Error: " + error);
        }

        private void determineCells(){
            grid = new int[(width + 1) * (height + 1)];

            for(int i = 0; i < height + 1; i++)
                for(int j = 0; j < width + 1; j++)
                    determineCell(i, j);
        }

        private void determineCell(int row, int column){
            int topLeft = data[row * (width + 2) + column];
            int topRight = data[row * (width + 2) + column + 1];
            int bottomLeft = data[(row + 1) * (width + 2) + column];
            int bottomRight = data[(row + 1) * (width + 2) + column + 1];

            grid[row * (width + 1) + column] = getCellValue(topLeft, topRight, bottomLeft, bottomRight);
        }

        private int getCellValue(int topLeft, int topRight, int bottomLeft, int bottomRight){
            return topLeft * 8 + topRight * 4 + bottomLeft * 2 + bottomRight;
        }

        private void createObstacles(){
            obstacleVertices = new List<LinkedList<Vector2>>();

            visited = new bool[(width + 1) * (height + 1)];

            for(int i = 0; i < height + 1; i++)
                for(int j = 0; j < width + 1; j++){
                    int index = i * (width + 1) + j;
                    
                    if(visited[index])
                        continue;
                    else if(grid[index] != 0 && grid[index] != 15)
                        createObstacle(i, j);
                }
        }

        private void createObstacle(int row, int column){
            LinkedList<Vector2> vertices = new LinkedList<Vector2>();

            int startIndex = row * (width + 1) + column;

            int currentRow = row;
            int currentColumn = column;

            int lastRow = row;
            int lastColumn = column;

            int counter = 0;

            int startValue = grid[startIndex];
            bool clockwise = startValue == 1 || startValue == 6 || startValue == 7;

            while(currentRow != row || currentColumn != column || !visited[startIndex]){
                int currentIndex = currentRow * (width + 1) + currentColumn;
                visited[currentIndex] = true;

                counter++;
                if(counter == 2000000) {
                    Debug.Log("Limit Break!");
                    break;
                }

                int leftValue = 0;
                int rightValue = 0;
                int topValue = 0;
                int bottomValue = 0;

                if(currentColumn > 0)
                    leftValue = grid[currentRow * (width + 1) + currentColumn - 1];
                
                if(currentColumn < width)
                    rightValue = grid[currentRow * (width + 1) + currentColumn + 1];
                
                if(currentRow > 0)
                    topValue = grid[(currentRow - 1) * (width + 1) + currentColumn];
                
                if(currentRow < height)
                    bottomValue = grid[(currentRow + 1) * (width + 1) + currentColumn];

                int tempRow = currentRow;
                int tempColumn = currentColumn;

                switch(grid[currentIndex]){
                    case 1:
                        if(clockwise){
                            if(
                                rightValue == 2 ||
                                rightValue == 3 ||
                                rightValue == 9 ||
                                rightValue == 12 ||
                                rightValue == 13
                              )
                                vertices.AddLast(new Vector2(currentColumn + 0.5f, height - currentRow));
                            currentColumn++;
                            break;
                        }else{
                            if(
                                bottomValue == 4 ||
                                bottomValue == 5 ||
                                bottomValue == 9 ||
                                bottomValue == 10 ||
                                bottomValue == 11
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow + 0.5f)));
                            currentRow++;
                            break;
                        }

                    case 2:
                        if(clockwise){
                            if(
                                bottomValue == 5 ||
                                bottomValue == 6 ||
                                bottomValue == 7 ||
                                bottomValue == 8 ||
                                bottomValue == 10
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow + 0.5f)));
                            currentRow++;
                            break;
                        }else{
                            if(
                                leftValue == 1 ||
                                leftValue == 3 ||
                                leftValue == 6 ||
                                leftValue == 12 ||
                                leftValue == 14
                              )
                                vertices.AddLast(new Vector2(currentColumn - 0.5f, height - currentRow));
                            currentColumn--;
                            break;
                        }

                    case 3:
                        if(clockwise){
                            if(
                                rightValue == 2 ||
                                rightValue == 6 ||
                                rightValue == 7 ||
                                rightValue == 8 ||
                                rightValue == 9 ||
                                rightValue == 13
                              )
                                vertices.AddLast(new Vector2(currentColumn + 0.5f, height - currentRow));
                            currentColumn++;
                            break;
                        }else{
                            if(
                                leftValue == 1 ||
                                leftValue == 4 ||
                                leftValue == 6 ||
                                leftValue == 9 ||
                                leftValue == 11 ||
                                leftValue == 14
                              )
                                vertices.AddLast(new Vector2(currentColumn - 0.5f, height - currentRow));
                            currentColumn--;
                            break;
                        }

                    case 4:
                        if(clockwise){
                            if(
                                topValue == 1 ||
                                topValue == 5 ||
                                topValue == 6 ||
                                topValue == 10 ||
                                topValue == 14
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow - 0.5f)));
                            currentRow--;
                            break;
                        }else{
                            if(
                                rightValue == 3 ||
                                rightValue == 6 ||
                                rightValue == 7 ||
                                rightValue == 8 ||
                                rightValue == 12
                              )
                                vertices.AddLast(new Vector2(currentColumn + 0.5f, height - currentRow));
                            currentColumn++;
                            break;
                        }

                    case 5:
                        if(clockwise){
                            if(
                                topValue == 1 ||
                                topValue == 2 ||
                                topValue == 6 ||
                                topValue == 9 ||
                                topValue == 13 ||
                                topValue == 14
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow - 0.5f)));
                            currentRow--;
                            break;
                        }else{
                            if(
                                bottomValue == 4 ||
                                bottomValue == 6 ||
                                bottomValue == 7 ||
                                bottomValue == 8 ||
                                bottomValue == 9 ||
                                bottomValue == 11
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow + 0.5f)));
                            currentRow++;
                            break;
                        }

                    case 6:
                        if(clockwise){
                            if(lastColumn < currentColumn){
                                if(
                                    topValue == 2 ||
                                    topValue == 5 ||
                                    topValue == 9 ||
                                    topValue == 10 ||
                                    topValue == 13
                                  )
                                    vertices.AddLast(new Vector2(currentColumn, height - (currentRow - 0.5f)));
                                currentRow--;
                                break;
                            }else{
                                if(
                                bottomValue == 4 ||
                                bottomValue == 5 ||
                                bottomValue == 9 ||
                                bottomValue == 10 ||
                                bottomValue == 11
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow + 0.5f)));
                            currentRow++;
                            break;
                            }
                        }else{
                            if(lastRow < currentRow){
                                if(
                                    leftValue == 3 ||
                                    leftValue == 4 ||
                                    leftValue == 9 ||
                                    leftValue == 11 ||
                                    leftValue == 12
                                  )
                                    vertices.AddLast(new Vector2(currentColumn - 0.5f, height - currentRow));
                                currentColumn--;
                                break;
                            }else{
                                if(
                                rightValue == 2 ||
                                rightValue == 3 ||
                                rightValue == 9 ||
                                rightValue == 12 ||
                                rightValue == 13
                              )
                                vertices.AddLast(new Vector2(currentColumn + 0.5f, height - currentRow));
                            currentColumn++;
                            break;
                            }
                        }

                    case 7:
                        if(clockwise){
                            if(
                                topValue == 2 ||
                                topValue == 5 ||
                                topValue == 9 ||
                                topValue == 10 ||
                                topValue == 13
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow - 0.5f)));
                            currentRow--;
                            break;
                        }else{
                            if(
                                leftValue == 3 ||
                                leftValue == 4 ||
                                leftValue == 9 ||
                                leftValue == 11 ||
                                leftValue == 12
                              )
                                vertices.AddLast(new Vector2(currentColumn - 0.5f, height - currentRow));
                            currentColumn--;
                            break;
                        }

                    case 8:
                        if(clockwise){
                            if(
                                leftValue == 3 ||
                                leftValue == 4 ||
                                leftValue == 9 ||
                                leftValue == 11 ||
                                leftValue == 12
                              )
                                vertices.AddLast(new Vector2(currentColumn - 0.5f, height - currentRow));
                            currentColumn--;
                            break;
                        }else{
                            if(
                                topValue == 2 ||
                                topValue == 5 ||
                                topValue == 9 ||
                                topValue == 10 ||
                                topValue == 13
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow - 0.5f)));
                            currentRow--;
                            break;
                        }

                    case 9:
                        if(clockwise){
                            if(lastRow < currentRow){
                                if(
                                    rightValue == 3 ||
                                    rightValue == 6 ||
                                    rightValue == 7 ||
                                    rightValue == 8 ||
                                    rightValue == 12
                                  )
                                    vertices.AddLast(new Vector2(currentColumn + 0.5f, height - currentRow));
                                currentColumn++;
                                break;
                            }else{
                                if(
                                    leftValue == 1 ||
                                    leftValue == 3 ||
                                    leftValue == 6 ||
                                    leftValue == 12 ||
                                    leftValue == 14
                                  )
                                    vertices.AddLast(new Vector2(currentColumn - 0.5f, height - currentRow));
                                currentColumn--;
                                break;
                            }
                        }else{
                            if(lastColumn < currentColumn){
                                if(
                                    bottomValue == 5 ||
                                    bottomValue == 6 ||
                                    bottomValue == 7 ||
                                    bottomValue == 8 ||
                                    bottomValue == 10
                                  )
                                    vertices.AddLast(new Vector2(currentColumn, height - (currentRow + 0.5f)));
                                currentRow++;
                                break;
                            }else{
                                if(
                                    topValue == 1 ||
                                    topValue == 5 ||
                                    topValue == 6 ||
                                    topValue == 10 ||
                                    topValue == 14
                                  )
                                    vertices.AddLast(new Vector2(currentColumn, height - (currentRow - 0.5f)));
                                currentRow--;
                                break;
                            }
                        }

                    case 10:
                        if(clockwise){
                            if(
                                bottomValue == 4 ||
                                bottomValue == 6 ||
                                bottomValue == 7 ||
                                bottomValue == 8 ||
                                bottomValue == 9 ||
                                bottomValue == 11
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow + 0.5f)));
                            currentRow++;
                            break;
                        }else{
                            if(
                                topValue == 1 ||
                                topValue == 2 ||
                                topValue == 6 ||
                                topValue == 9 ||
                                topValue == 13 ||
                                topValue == 14
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow - 0.5f)));
                            currentRow--;
                            break;
                        }

                    case 11:
                        if(clockwise){
                            if(
                                rightValue == 3 ||
                                rightValue == 6 ||
                                rightValue == 7 ||
                                rightValue == 8 ||
                                rightValue == 12
                              )
                                vertices.AddLast(new Vector2(currentColumn + 0.5f, height - currentRow));
                            currentColumn++;
                            break;
                        }else{
                            if(
                                topValue == 1 ||
                                topValue == 5 ||
                                topValue == 6 ||
                                topValue == 10 ||
                                topValue == 14
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow - 0.5f)));
                            currentRow--;
                            break;
                        }

                    case 12:
                        if(clockwise){
                            if(
                                leftValue == 1 ||
                                leftValue == 4 ||
                                leftValue == 6 ||
                                leftValue == 9 ||
                                leftValue == 11 ||
                                leftValue == 14
                              )
                                vertices.AddLast(new Vector2(currentColumn - 0.5f, height - currentRow));
                            currentColumn--;
                            break;
                        }else{
                            if(
                                rightValue == 2 ||
                                rightValue == 6 ||
                                rightValue == 7 ||
                                rightValue == 8 ||
                                rightValue == 9 ||
                                rightValue == 13
                              )
                                vertices.AddLast(new Vector2(currentColumn + 0.5f, height - currentRow));
                            currentColumn++;
                            break;
                        }

                    case 13:
                        if(clockwise){
                            if(
                                leftValue == 1 ||
                                leftValue == 3 ||
                                leftValue == 6 ||
                                leftValue == 12 ||
                                leftValue == 14
                              )
                                vertices.AddLast(new Vector2(currentColumn - 0.5f, height - currentRow));
                            currentColumn--;
                            break;
                        }else{
                            if(
                                bottomValue == 5 ||
                                bottomValue == 6 ||
                                bottomValue == 7 ||
                                bottomValue == 8 ||
                                bottomValue == 10
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow + 0.5f)));
                            currentRow++;
                            break;
                        }

                    case 14:
                        if(clockwise){
                            if(
                                bottomValue == 4 ||
                                bottomValue == 5 ||
                                bottomValue == 9 ||
                                bottomValue == 10 ||
                                bottomValue == 11
                              )
                                vertices.AddLast(new Vector2(currentColumn, height - (currentRow + 0.5f)));
                            currentRow++;
                            break;
                        }else{
                            if(
                                rightValue == 2 ||
                                rightValue == 3 ||
                                rightValue == 9 ||
                                rightValue == 12 ||
                                rightValue == 13
                              )
                                vertices.AddLast(new Vector2(currentColumn + 0.5f, height - currentRow));
                            currentColumn++;
                            break;
                        }

                    default:
                        Debug.LogError("Shouldn't be at " + currentRow + " " + currentColumn + " " + grid[currentIndex]);
                        break;
                }

                lastRow = tempRow;
                lastColumn = tempColumn;
            }

            obstacleVertices.Add(vertices);
        }
    }
}
