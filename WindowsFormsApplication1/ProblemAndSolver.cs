using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Diagnostics;

namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }
            /*String stored = "maxStoredStates: " + maxStatesStored;
            String numStates = "numStatesCreated: " + numStatesCreated;
            String pruned = "States Pruned: " + numStatesPruned;
            System.Drawing.Font drawFont = new System.Drawing.Font(
        "Arial", 16);
            System.Drawing.SolidBrush drawBrush = new
        System.Drawing.SolidBrush(System.Drawing.Color.Black);
            g.DrawString(stored, drawFont, drawBrush, 30f, 30f);
            g.DrawString(numStates, drawFont, drawBrush, 30f, 130f);
            g.DrawString(pruned, drawFont, drawBrush, 30f, 230f);*/

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }
        int numStatesCreated = 1;
        int numStatesPruned = 1;
        int maxStatesStored = 1;
        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {

            //make first TSPath and add it to the binary heap
            MinHeap<TSPath> heap = new MinHeap<TSPath>();
            
            int timeout = TIME_LIMIT * 1000;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            heap.Insert(new TSPath(GetCities()));
            MinHeap<TSDistance> minDistances = new MinHeap<TSDistance>();
            minDistances.Insert(new TSDistance(heap.Peek()));
            double bestPossibleSolution = heap.Peek().getDistance();
            int numSolutions = 0;
            defaultSolveProblem();
            double bestSolutionDistance = costOfBssf();
            int[] bssfArray = new int[0];
            numStatesCreated = 1;
            numStatesPruned = 1;
            maxStatesStored = 1;
            //repeat following until timeout or best solution found
            while ( heap.Count != 0 && stopWatch.ElapsedMilliseconds < timeout)
            {

                maxStatesStored = (maxStatesStored > heap.Count ? maxStatesStored : heap.Count);
                TSPath min = heap.ExtractMin();
                if (min.isFinished)
                {
                    numSolutions++;
                    ArrayList Route = new ArrayList();
                    City[] cities = GetCities();
                    int[] minArray = min.getPath();
                    for (int i = 0; i < minArray.Length; i++)
                    {
                        Route.Add(cities[minArray[i]]);
                    }
                    double minLength = min.getDistance();
                    
                    TSPSolution solution = new TSPSolution(Route);

                    if (solution.costOfRoute() < bestSolutionDistance)
                    {
                        bssfArray = min.getPath();
                        bestSolutionDistance = solution.costOfRoute();
                        bssf = solution;
                    }
                } else
                {
                    if (min.getDistance() > bestSolutionDistance)
                    {
                        numStatesPruned++;
                    }
                    else
                    {
                        foreach (TSPath child in min.getAllChildren())
                        {
                            numStatesCreated++;
                            if (child.getDistance() < bestSolutionDistance)
                            {
                                //minDistances.Insert(new TSDistance(child));
                                
                                heap.Insert(child);
                            } else
                            {
                                numStatesPruned++;
                            }
                        }
                    }
                }
            }
            string[] results = new string[3];
            
            /*if(numSolutions > 0)
            {
                ArrayList Route = new ArrayList();
                City[] cities = GetCities();
                for(int i = 0; i < bssfArray.Length; i++)
                {
                    Route.Add(cities[bssfArray[i]]);
                }
                bssf = new TSPSolution(Route);
            }*/

            results[COST] = costOfBssf().ToString();    // load results into array here, replacing these dummy values
            results[TIME] = (((double)stopWatch.ElapsedMilliseconds) / 1000).ToString();
            results[COUNT] = numSolutions.ToString() ;
            
            
            return results;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        private Boolean[] citiesVisited;
        public string[] greedySolveProblem()
        {
            string[] results = new string[3];
            var watch = System.Diagnostics.Stopwatch.StartNew();

            for(int i = 0; i < Cities.Length; i++)
            {
                TSPSolution temp = greedySolveProblem(i);
                double tempLength = temp.costOfRoute();
                double bssfLength = costOfBssf();
                if (bssf == null)
                {
                    bssf = temp;
                }
                else
                {
                    if (temp.costOfRoute() < costOfBssf())
                    {
                        bssf = temp;
                    }
                }
            }

            results[COST] = bssf.costOfRoute().ToString();    // load results into array here, replacing these dummy values
            results[TIME] = watch.Elapsed.ToString();
            results[COUNT] = "1";
            return results;
        }
        private TSPSolution greedySolveProblem(int startingIndex)
        {
            
            // TODO: Add your implementation for a greedy solver here.
            citiesVisited = new Boolean[Cities.Length];


            //double costSoFar = Double.PositiveInfinity;
            Route = new ArrayList();
            Route.Add(Cities[startingIndex]);
            citiesVisited[startingIndex] = true;
            int lastCity = startingIndex;
            while (Route.Count < Cities.Length)
            {
                int nextCity = 0;
                double lowestCost = Double.PositiveInfinity;
                for (int i = 0; i < Cities.Length; i++)
                {
                    if (citiesVisited[i])
                    {
                        continue;
                    }
                    else
                    {
                        double cost = Cities[lastCity].costToGetTo(Cities[i]);
                        if (Cities[lastCity].costToGetTo(Cities[i]) < lowestCost)
                        {
                            nextCity = i;
                            lowestCost = cost;
                        }
                    }
                }
                citiesVisited[nextCity] = true;
                Route.Add(Cities[nextCity]);
                lastCity = nextCity;
            }
            

            return new TSPSolution(Route);

            
        }

        private int Q = 500;
        private double DECAY_COEF = .5;
        private double BETA = 5;
        private double ALPHA = 1;
        private double STARTING_PHER = 1.0;
        private int NUM_ITERATIONS = 2000;
        private double RANDOM_CHANCE = .03;

        private int numAnts = 0;
        private Random rand = new Random();

        private double[,] pher;
        private Ant[] ants;

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];
            var watch = System.Diagnostics.Stopwatch.StartNew();
            pher = new double[Cities.Length, Cities.Length];

            int iteration = 0;
            int updates = 0;
            initializePher();

            while (iteration < NUM_ITERATIONS)
            {
                initializeAnts();
                for (int i = 0; i < numAnts; i++)
                {
                    doCircut(ants[i]);
                }
                PherDecay();
                pherUpdate();
                TSPSolution tempSolution = getShortestTour();
                if (bssf == null)
                {
                    updates++;
                    bssf = tempSolution;
                }
                if (tempSolution.costOfRoute() <= bssf.costOfRoute())
                {
                    updates++;
                    bssf = tempSolution;
                }
                iteration++;
            }

            watch.Stop();

            results[COST] = bssf.costOfRoute().ToString();    // load results into array here, replacing these dummy values
            results[TIME] = watch.Elapsed.ToString();
            results[COUNT] = updates.ToString();

            return results;
        }

        private TSPSolution getShortestTour()
        {
            double lowestCost = Double.PositiveInfinity;
            Ant lowestAnt = null;
            foreach (Ant ant in ants)
            {
                if (ant.getFinalTourLength(Cities) < lowestCost)
                {
                    lowestCost = ant.getFinalTourLength(Cities);
                    lowestAnt = ant;
                }
            }
            if (lowestAnt == null)
            {
                return null;
            }
            return new TSPSolution(indexToCities(Cities, lowestAnt.pathSoFar));
        }

        public void initializePher()
        {
            for (int i = 0; i < Cities.Length; i++)
            {
                for (int j = 0; j < Cities.Length; j++)
                {
                    pher[i, j] = STARTING_PHER;
                }
            }
        }

        public void initializeAnts()
        {
            numAnts = Cities.Length;
            ants = new Ant[numAnts];
            for (int i = 0; i < numAnts; i++)
            {
                ants[i] = new Ant(Cities.Length);
            }
        }

        public void doCircut(Ant ant)
        {
            ant.visitCity(rand.Next(0, Cities.Length));
            for (int i = 0; i < Cities.Length - 1; i++)
            {

                if (rand.NextDouble() < RANDOM_CHANCE)
                {
                    int t = rand.Next(Cities.Length - ant.getCurrentCity()); // random town
                    int j = -1;
                    for (int k = 0; k < Cities.Length; k++)
                    {
                        if (!ant.citiesVisited[k])
                            j++;
                        if (j == t)
                        {
                            ant.visitCity(k);
                            break;
                        }
                    }
                }

                double[] probs = new double[Cities.Length];
                double denom = 0.0;

                for (int j = 0; j < Cities.Length; j++)
                {
                    if (ant.citiesVisited[j])
                    {
                        probs[j] = 0.0;
                    }
                    else
                    {

                        probs[j] = getProbability(ant.getCurrentCity(), j);
                    }
                    denom += probs[j];
                }

                if (denom == 0)
                {
                    ant = new Ant(Cities.Length);
                    doCircut(ant);
                    if (ant.pathSoFar.Count < Cities.Length)
                    {
                        finishPath(ant);
                        return;
                    }
                    return;
                }
                for (int j = 0; j < Cities.Length; j++)
                {
                    probs[j] /= denom;
                }
                double r = rand.NextDouble();
                double total = 0.0;
                for (int j = 0; j < Cities.Length; j++)
                {
                    total += probs[j];
                    if (total >= r)
                    {
                        ant.visitCity(j);
                        break;
                    }
                }
            }
            if (ant.pathSoFar.Count < Cities.Length)
            {
                finishPath(ant);
                return;
            }
        }

        private void finishPath(Ant ant)
        {
            while (ant.pathSoFar.Count < Cities.Length)
            {
                for (int i = 0; i < Cities.Length; i++)
                {
                    if (!ant.citiesVisited[i])
                    {
                        ant.visitCity(i);
                    }
                }
            }
        }

        public double getProbability(int city1, int city2)
        {
            double pherLevel = pher[city1, city2];
            double dist = Cities[city1].costToGetTo(Cities[city2]);
            if (!Double.IsPositiveInfinity(dist))
            {
                return (Math.Pow(pherLevel, ALPHA) * Math.Pow((1 / dist), BETA));
            }
            else
            {
                return 0.0;
            }
        }

        public void PherDecay()
        {
            for (int i = 0; i < Cities.Length; i++)
            {
                for (int j = 0; j < Cities.Length; j++)
                {
                    pher[i, j] *= DECAY_COEF;
                }
            }
        }

        public void pherUpdate()
        {
            foreach (Ant ant in ants)
            {
                try
                {
                    double con = Q / ant.getFinalTourLength(Cities);
                    for (int i = 0; i < Cities.Length - 1; i++)
                    {
                        pher[ant.pathSoFar[i], ant.pathSoFar[i + 1]] += con;
                    }
                    pher[ant.pathSoFar[Cities.Length - 1], ant.pathSoFar[0]] += con;
                }
                catch
                {

                }
            }
        }

        public class Ant
        {
            public bool[] citiesVisited;
            public List<int> pathSoFar;

            public Ant(int numberOfCities)
            {
                citiesVisited = new bool[numberOfCities];
                pathSoFar = new List<int>();
            }

            public void visitCity(int cityIndex)
            {
                citiesVisited[cityIndex] = true;
                pathSoFar.Add(cityIndex);
            }

            public double getFinalTourLength(City[] cities)
            {
                try
                {
                    TSPSolution solution = new TSPSolution(indexToCities(cities, pathSoFar));
                    return solution.costOfRoute();
                }
                catch
                {
                    return Double.PositiveInfinity;
                }
            }

            public int getCurrentCity()
            {
                return pathSoFar[pathSoFar.Count - 1];
            }


        }


        public static ArrayList indexToCities(City[] cities, List<int> indexs)
        {
            ArrayList citiesList = new ArrayList();
            ArrayList arrayIndexs = new ArrayList(indexs);
            for (int i = 0; i < cities.Length; i++)
            {
                citiesList.Add(cities[(int)arrayIndexs[i]]);
            }
            return citiesList;
        }




        
    }

    #endregion
}
