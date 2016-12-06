using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class TSDistance : IComparable
    {
        public double minDistance;
        public int[] path;
        public TSDistance(TSPath path)
        {
            this.path = path.cityPathOrder;
            minDistance = path.getDistance();
        }
        public int CompareTo(Object obj)
        {
            if (obj == null) return 1;

            TSDistance other = obj as TSDistance;
            if (other != null)
                return this.minDistance.CompareTo(other.minDistance);
            else
                throw new ArgumentException("Object is not a TSPath");
        }
    }
    class TSPath : IComparable
    {
        protected double score;
        private double minDistance;
        protected int level;
        protected double[,] distances;
        public int[] cityPathOrder;
        private TSPath parent;//to get the path back home
        protected int destination;
        static protected int numCities;
        public bool isFinished;
        // used to initialize each city
        public TSPath(City[] cities)
        {
            isFinished = false;
            parent = null;
            minDistance = 0;
            level = 1;
            numCities = cities.Length;
            distances = new double[numCities,numCities];
            cityPathOrder = new int[numCities];
            for(int i = 0; i < numCities; i++)
            {
                City refCity = cities[i];
                for(int j = 0; j < numCities; j++)
                {
                    double distance = refCity.costToGetTo(cities[j]);
                    distances[i, j] = distance;
                }
                distances[i, i] = double.PositiveInfinity;
                cityPathOrder[i] = 0;
            }
            cityPathOrder[0] = 1;
            commonInitialization();
        }

        public TSPath(TSPath parent, int destination)
        {
            isFinished = false;
            this.destination = destination;
            this.parent = parent;
            int sourceCity = parent.destination;
            minDistance = parent.minDistance + parent.distances[sourceCity, this.destination];
            level = parent.level + 1;
            cityPathOrder = (int[])parent.cityPathOrder.Clone();
            cityPathOrder[destination] = level;
            if (parent.cityPathOrder[destination] > 0)
            {
                if(level == numCities + 1 && destination == 0)
                {
                    cityPathOrder[0] = 1;//this is to reset the first city to being first not last
                    //minDistance += parent.distances[sourceCity, destination];
                    score = 0;
                    isFinished = true;
                    return;
                }
                minDistance = double.PositiveInfinity;
                score = double.PositiveInfinity;
                return;
            }
            if (double.IsPositiveInfinity(parent.distances[sourceCity, destination]))
            {
                minDistance = double.PositiveInfinity;
                score = double.PositiveInfinity;
                return;
            }
            distances = (double[,])parent.distances.Clone();
            commonInitialization();
        }
        /**
         * returns all cities that haven't been searched yet or the first city
         */
        public List<TSPath> getAllChildren()
        {
            List<TSPath> children = new List<TSPath>();
            if (level == numCities)
            {
                children.Add(new TSPath(this, 0));
                return children;
            }
            for(int i = 1; i < numCities; i++)
            {
                if(cityPathOrder[i] == 0)
                {
                    children.Add(new TSPath(this, i));
                }
            }
            return children;
        }
        private void commonInitialization()
        {
            ReduceGraph();
            calculateScore();
        }
        public double getDistance()
        {
            return minDistance;
        }
        private void ReduceGraph()
        {

            //zero out rows
            for (int i = 0; i < numCities; i++)
            {
                if (cityPathOrder[i] == 0)
                {
                    double minInRow = double.PositiveInfinity;
                    for (int j = 0; j < numCities; j++)
                    {
                        if (distances[i, j] < minInRow)
                            minInRow = distances[i, j];
                    }
                    if (minInRow > 0 && !double.IsPositiveInfinity(minInRow))
                    {
                        minDistance += minInRow;
                        for (int j = 0; j < numCities; j++)
                        {
                            if (!double.IsPositiveInfinity(distances[i, j]))
                            {
                                distances[i, j] -= minInRow;
                            }
                        }
                    } else if(double.IsPositiveInfinity(minInRow))
                    { // if the row hasn't been visited yet we have a problem
                        minDistance = double.PositiveInfinity;
                        return;
                    }
                }
            }
            // zero out columns
            for(int i = 0; i < numCities; i++)
            {
                // check 1 because we still need to get there at the end
                if (cityPathOrder[i] <= 1)
                {
                    double minInColumn = double.PositiveInfinity;
                    for (int j = 0; j < numCities; j++)
                    {
                        if (distances[j, i] < minInColumn)
                        {
                            minInColumn = distances[j, i];
                        }
                    }
                    if (minInColumn > 0 && !double.IsPositiveInfinity(minInColumn))
                    {
                        minDistance += minInColumn;
                        for (int j = 0; j < numCities; j++)
                        {
                            if (!double.IsPositiveInfinity(distances[j, i]))
                            {
                                distances[j, i] -= minInColumn;
                            }
                        }
                    } else if (double.IsPositiveInfinity(minInColumn))
                    {
                        minDistance = double.PositiveInfinity;
                    }
                }
            }
        }
        public int[] getPath()
        {
            if (!isFinished)
            {
                return null;
            }
            int[] res = new int[numCities];
            for(int i = 0; i < numCities; i++)
            {
                res[cityPathOrder[i]-1] = i;
            }
            return res;
        }
        private void calculateScore()
        {
            if (double.IsPositiveInfinity(minDistance))
            {
                score = double.PositiveInfinity;
                return;
            }
            score = minDistance / (level);
        }
        public int CompareTo(Object obj)
        {
            if (obj == null) return 1;

            TSPath otherTemperature = obj as TSPath;
            if (otherTemperature != null)
                return this.score.CompareTo(otherTemperature.score);
            else
                throw new ArgumentException("Object is not a TSPath");
        }
    }
}
