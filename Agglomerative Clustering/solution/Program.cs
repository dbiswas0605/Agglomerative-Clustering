﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class Solution 
{
    enum SIMILARITYMEASURE
    {
        SINGLE,COMPLETE,AVERAGE
    }
    class ClusterHelper
    {
        const string path = @"C:\Users\d.biswas\Desktop\MCS - DS\Program\Agglomerative Clustering\solution\Test Data\";
        public static List<Point> _points = new List<Point>();
        public static int _numberOfCluster = 0;
        public static int _recCount = 0;
        public static SIMILARITYMEASURE _similaritymeasure = SIMILARITYMEASURE.SINGLE;
        public static Dictionary<string, double> _distancePair = new Dictionary<string,double>();

        public static void LoadPointsFromFile(string file)
        {
            var lines = File.ReadAllLines($"{path}{file}");
            int lineNum = -1;
            foreach(var line in lines)
            {
                if(lineNum==-1)
                {
                    var header = line.Split(' ');

                    _recCount = Convert.ToInt32(header[0]);
                    _numberOfCluster = Convert.ToInt32(header[1]);
                    _similaritymeasure = (SIMILARITYMEASURE) Enum.Parse(typeof(SIMILARITYMEASURE) ,header[2]);

                    lineNum++;
                }
                else
                {
                    double lat = Convert.ToDouble(line.Split(' ')[0]);
                    double lon = Convert.ToDouble(line.Split(' ')[1]);

                    _points.Add(new Point(lat,lon, lineNum++));
                }
            }
        }

        private static double GetEuclidianDistance(Point a, Point b)
        {
            double dist = Math.Sqrt((a.Latitude - b.Latitude) * (a.Latitude - b.Latitude) +
                    (a.Longitude - b.Longitude) * (a.Longitude - b.Longitude));

            return dist;
        }

        public static void ComputeDistanceBetweenEachPair()
        {
            Point[] _pnts = _points.ToArray();
            int items = _pnts.Length;

            for(int i=0;i<items-1;i++)
            {
                for(int j=i+1; j<items; j++)
                {
                    double dist = GetEuclidianDistance(_pnts[i], _pnts[j]);
                    _distancePair.Add($"{_pnts[i].Sequence}<->{_pnts[j].Sequence}",dist);
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
        public int _targetClusters = 0;
        public SIMILARITYMEASURE _similaritymeasure=SIMILARITYMEASURE.SINGLE;        

        public void InitializeCluster(string sampleFileName)
        {
            ClusterHelper.LoadPointsFromFile(sampleFileName);
            ClusterHelper.ComputeDistanceBetweenEachPair();

            _targetClusters = ClusterHelper._numberOfCluster;
            _similaritymeasure = ClusterHelper._similaritymeasure;

            foreach(Point _p in ClusterHelper._points)
            {
                Cluster c = new Cluster(getNextClusterID(),_p);
                _cluster.Add(c);
            }
        }

        public Tuple<string,string,double> RunSingleLinkCluster(Cluster[] c)
        {
            Dictionary<string,double> clusterDistance = new Dictionary<string, double>();
            for(int i=0; i<c.Length-1;i++)
            {
                for(int j=i+1; j<c.Length; j++)
                {
                    var dist = GetMinClusterDistance(c[i], c[j]);
                    clusterDistance.Add($"{c[i]._clusterid}<->{c[j]._clusterid}",dist);
                }
            }

            var minVal = clusterDistance.Values.Min();
            var closestCluster = clusterDistance.Where(x=>x.Value==minVal).First().Key;
            
            var id1 = closestCluster.Split("<->")[0];
            var id2 = closestCluster.Split("<->")[1];

            return Tuple.Create(id1,id2,minVal);
        }

        public Tuple<string,string,double> RunCompleteLinkCluster(Cluster[] c)
        {
            Dictionary<string,double> clusterDistance = new Dictionary<string, double>();
            for(int i=0; i<c.Length-1;i++)
            {
                for(int j=i+1; j<c.Length; j++)
                {
                    var dist = GetMinClusterDistance(c[i], c[j]);
                    clusterDistance.Add($"{c[i]._clusterid}<->{c[j]._clusterid}",dist);
                }
            }

            var maxVal = clusterDistance.Values.Max();
            var closestCluster = clusterDistance.Where(x=>x.Value==maxVal).First().Key;
            
            var id1 = closestCluster.Split("<->")[0];
            var id2 = closestCluster.Split("<->")[1];

            return Tuple.Create(id1,id2,maxVal);            
        }        

        public void AddPointToClusterByID(Point p, int clusterID)
        {
            Cluster c = _cluster.Where(x=>x._clusterid == clusterID).First();
            c._points.Add(p);
            p.ClusterID=c._clusterid;
            _cluster.Add(c);
        }
        public void AddPointsToClusterByID(Point[] p, int clusterID)
        {
            Cluster c = _cluster.Where(x=>x._clusterid == clusterID).First();
            c._points.AddRange(p);

            foreach(var _p in p)
                _p.ClusterID=c._clusterid;
            
            _cluster.Add(c);
        }

        public Cluster MergeCluster(Cluster c1, Cluster c2)
        {
            var p1 = c1._points;
            var p2 = c2._points;

            p1.AddRange(p2);
            var p = p1.ToArray();

            Cluster c = new Cluster(getNextClusterID(),p);

            foreach(var _p in p)
                _p.ClusterID=c._clusterid;

            return c;
        }

        public void DeleteCluster(int id)
        {
            var c = _cluster.Where(x=>x._clusterid==id).First();
            _cluster.Remove(c);
        } 

        public double GetMinClusterDistance(Cluster a, Cluster b)
        {
            List<double> interDistance = new List<double>();

            foreach(Point av in a._points)
            {
                foreach(Point bw in b._points)
                {
                    string key = $"{av.Sequence}<->{bw.Sequence}";
                    var d = ClusterHelper._distancePair[key];

                    interDistance.Add(d);
                }
            }

            return interDistance.Min();
        }

        public double GetMaxClusterDistance(Cluster a, Cluster b)
        {
            List<double> interDistance = new List<double>();

            foreach(Point av in a._points)
            {
                foreach(Point bw in b._points)
                {
                    string key = $"{av.Sequence}<->{bw.Sequence}";
                    var d = ClusterHelper._distancePair[key];

                    interDistance.Add(d);
                }
            }

            return interDistance.Max();
        }

        public double GetMeanClusterDistance(Cluster a, Cluster b)
        {
            List<double> interDistance = new List<double>();

            foreach(Point av in a._points)
            {
                foreach(Point bw in b._points)
                {
                    string key = $"{av.Sequence}<->{bw.Sequence}";
                    var d = ClusterHelper._distancePair[key];

                    interDistance.Add(d);
                }
            }

            return interDistance.Average();
        }

        internal static int getNextClusterID()
        {
            return clusterid++;
        }
    }


    static void Main(String[] args) {
        /* Enter your code here. Read input from STDIN. Print output to STDOUT. Your class should be named Solution */
        
        ClusterHandler ch = new ClusterHandler();
        ch.InitializeCluster("small2single.txt");




        
        var d2 = ClusterHelper._distancePair;
        
        Console.WriteLine("yo");







    }
}