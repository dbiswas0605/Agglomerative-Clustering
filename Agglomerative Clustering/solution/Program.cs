using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class Solution 
{

    class SimpleLinkHelper
    {
        private List<Point> _points = new List<Point>();
        private Dictionary<Tuple<Point, Point>,double> _distancePair = new Dictionary<Tuple<Point, Point>,double>();

        public List<Point> Points { get => _points; set => _points = value; }

        public void LoadPointsFromFile()
        {
            var lines = File.ReadAllLines("locations.txt");
            int lineNum = 0;
            foreach(var line in lines)
            {
                double lat = Convert.ToDouble(line.Split(',')[0]);
                double lon = Convert.ToDouble(line.Split(',')[1]);

                _points.Add(new Point(lat,lon, lineNum++));
            }
        }

        private double GetEuclidianDistance(Point a, Point b)
        {
            double dist = Math.Sqrt((a.Latitude - b.Latitude) * (a.Latitude - b.Latitude) +
                    (a.Longitude - b.Longitude) * (a.Longitude - b.Longitude));

            return dist;
        }

        private void ComputeDistanceBetweenEachPair()
        {
            Point[] _pnts = _points.ToArray();
            int items = _pnts.Length;

            for(int i=0;i<items-1;i++)
            {
                for(int j=i+1; j<items; j++)
                {
                    double dist = GetEuclidianDistance(_pnts[i], _pnts[j]);
                    _distancePair.Add(Tuple.Create(_pnts[i], _pnts[j]),dist);
                }
            }
        }
    }
    class Point
    {
        double latitude;
        double longitude;
        int clusterID;
        readonly int sequence;

        public double Latitude { get => latitude; set => latitude = value; }
        public double Longitude { get => longitude; set => longitude = value; }
        public int ClusterID { get => clusterID; set => clusterID = value; }

        public int Sequence => sequence;

        internal Point(double lat, double lon, int position)
        {
            latitude=lat;
            Longitude=lon;
            ClusterID=position;
            sequence=position;
        }
    }


    class Cluster
    {
        public int _clusterid;
        public List<Point> _points = new List<Point>();

        internal Cluster(int id, Point p)
        {
            _clusterid=id;
            _points.Add(p);
            p.ClusterID=id;
        }

        internal Cluster(int id, Point[] p)
        {
            _clusterid=id;

            foreach(var _p in p)
                _p.ClusterID=id;

            _points.AddRange(p);
        }        
    }


    class ClusterHandler
    {
        public List<Cluster> _cluster = new List<Cluster>();
        private static int clusterid;

        public void AddPointToCluster(Point p, int clusterID)
        {
            Cluster c = new Cluster(getNextClusterID(),p);
            p.ClusterID=c._clusterid;
            _cluster.Add(c);
        }
        public void AddPointsToCluster(Point[] p, int clusterID)
        {
            Cluster c = new Cluster(getNextClusterID(),p);
            
            foreach(var _p in p)
                _p.ClusterID=c._clusterid;
            
            _cluster.Add(c);
        }

        public void MergeCluster(Cluster c1, Cluster c2)
        {
            var p1 = c1._points;
            var p2 = c2._points;

            p1.AddRange(p2);
            var p = p1.ToArray();

            Cluster c = new Cluster(getNextClusterID(),p);

            foreach(var _p in p)
                _p.ClusterID=c._clusterid;
        }

        public void DeleteCluster(int id)
        {
            var c = _cluster.Where(x=>x._clusterid==id).First();
            _cluster.Remove(c);
        }        

        internal static int getNextClusterID()
        {
            return clusterid++;
        }
    }


    static void Main(String[] args) {
        /* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */
        Console.WriteLine("yo");







    }
}