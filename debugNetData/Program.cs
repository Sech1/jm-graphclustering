using NetMining.ClusteringAlgo;
using NetMining.Graphs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace debugNetData
{
    class Program
    {
        static void Main(string[] args)
        {
            String workingDir = Directory.GetCurrentDirectory();
            String datapath = workingDir + "/Data";
            if (!Directory.Exists(datapath))
            {
                Directory.CreateDirectory(datapath);
                Console.WriteLine("Please move <healthyfile> and/or <infectedfile> to: " + datapath);
                Environment.Exit(0);
            }

            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Usage: Program.cs <Healthyfile> <Infectedfile> <Group> ");
                Environment.Exit(0);
            }

            // AUTOMATING IBD 
            // We need both a healthy network and an IBD network
            // COMMAND LINE: clusteringanalysis.exe healthyNet infectedNet VATorINTorTEN  


            //convert from gml to graph
            
            String healthyfile;
            String infectedfile;

            try
            {
                healthyfile = args[0];
            }
            catch (Exception e)
            {
                healthyfile = Path.GetFileName(args[0]);
            }

            try
            {
                infectedfile = args[1];
            }
            catch(Exception e)
            {
                infectedfile = Path.GetFileName(args[1]);
            }

            LightWeightGraph healthy = LightWeightGraph.GetGraphFromGML($"{healthyfile}");
            LightWeightGraph infected = LightWeightGraph.GetGraphFromGML($"{infectedfile}");
            healthyfile = healthyfile.Split('.')[0];
            infectedfile = infectedfile.Split('.')[0];


            if (healthyfile.Contains("/"))
            {
                healthyfile = healthyfile.Split('/').Last();
                healthyfile = datapath + "/" + healthyfile;
            }
            if (healthyfile.Contains("\\"))
            {
                healthyfile = healthyfile.Split('\\').Last();
                healthyfile = datapath + "\\" + healthyfile;
            }

            if (infectedfile.Contains("/"))
            {
                infectedfile = infectedfile.Split('/').Last();
                infectedfile = datapath + "/" + infectedfile;
            }
            if (infectedfile.Contains("\\"))
            {
                infectedfile = infectedfile.Split('\\').Last();
                infectedfile = datapath + "\\" + infectedfile;
            }

            healthy.SaveGraph(healthyfile + ".graph");
            infected.SaveGraph(infectedfile + ".graph");
            // Makes a list of what the nodes reference
            using (StreamWriter sw = new StreamWriter(healthyfile + ".txt", true))
            {
                for (int i = 0; i < healthy.Nodes.Length; i++)
                {
                    sw.WriteLine(healthy.Nodes[i].sharedName);
                }
            }

            using (StreamWriter sw = new StreamWriter(infectedfile + ".txt", true))
            {
                for (int i = 0; i < infected.Nodes.Length; i++)
                {
                    sw.WriteLine(infected.Nodes[i].sharedName);
                }
            }

            //we don't actually know the number of clusters in each graph - we want to cluster for 1 more than we start with
            //so cluster for 1 just to get the file.
            //HVATClust clust1 = new HVATClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);

            HVATClust healthyClust1 = new HVATClust(healthy, 1, false, 1, 0, false, false);
            Partition t1 = healthyClust1.GetPartition();
            int healthyClusters = t1.Clusters.Count;
            HVATClust infectedClust1 = new HVATClust(infected, 1, false, 1, 0, false, false);
            Partition t2 = infectedClust1.GetPartition();
            int infectedClusters = t2.Clusters.Count;

            // Now we know the intital number of clusters, do the actual clustering
            //HVATClust clust1 = new HVATClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);

            // This sees if the input cluster type can be parsed as the Enum, and if so 
            // Uses a switch statement to decide which clustering to run.
            if (args.Length == 3)
            {
                List<DataOutStruct> outData = ConstructList(args[2], healthy, infected, healthyfile, infectedfile,
                    healthyClusters, infectedClusters);
                using (StreamWriter sw = new StreamWriter(datapath + "/" + args[2] + ".csv"))
                {
                    for (int i = 0; i < outData.Count(); i++)
                        sw.WriteLine(outData[i].Bacteria + ", " + outData[i].GroupNum);
                }
            }
            else
            {
                Console.WriteLine(
                    "Please enter 3 parameters, in this order:\n " +
                    "Healthy data path(.gml)\n " +
                    "Unhealthy data path(.gml)\n " +
                    "Desired Output Group(listed in Readme)\n");
            }
        }

        private static List<DataOutStruct> ConstructList(string args, LightWeightGraph healthy,
            LightWeightGraph infected, String healthyfile, String infectedfile, int healthyClusters,
            int infectedClusters)
        {
            List<List<DataOutStruct>> outList = new List<List<DataOutStruct>>();
            List<DataOutStruct> dataOut = new List<DataOutStruct>();
            if (Enum.TryParse<ClusterType>(args, ignoreCase: true, result: out var userOut))
            {
                GeneralCluster cluster = ReturnClusterAndPartition(healthy, infected, healthyClusters, infectedClusters,
                    healthyfile, infectedfile);
                List<DataOutStruct> d1;
                List<DataOutStruct> d2;
                List<DataOutStruct> d3;
                switch (userOut)
                {
                    case ClusterType.G1I:
                        dataOut = G1(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile, OutType.Int,
                            outList));
                        break;
                    case ClusterType.G1T:
                        dataOut = G1(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyTenCount, cluster.InfectedTenCount, healthyfile, infectedfile, OutType.Ten,
                            outList));
                        break;
                    case ClusterType.G1V:
                        dataOut = G1(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile, OutType.Vat,
                            outList));
                        break;
                    case ClusterType.G2I:
                        dataOut = G2(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition, cluster.HealthyIntCount,
                                cluster.InfectedIntCount, healthyfile, infectedfile, OutType.Int, outList),
                            cluster.Int0.Partition, cluster.Int1.Partition, OutType.Int);
                        break;
                    case ClusterType.G2T:
                        dataOut = G2(
                            GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition, cluster.HealthyTenCount,
                                cluster.InfectedTenCount, healthyfile, infectedfile, OutType.Ten, outList),
                            cluster.Ten0.Partition, cluster.Ten1.Partition, OutType.Ten);
                        break;
                    case ClusterType.G2V:
                        dataOut = G2(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition, cluster.HealthyVatCount,
                                cluster.InfectedVatCount, healthyfile, infectedfile, OutType.Vat, outList),
                            cluster.Vat0.Partition, cluster.Vat1.Partition, OutType.Vat);
                        break;
                    case ClusterType.G3I:
                        dataOut = G3(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition, cluster.HealthyIntCount,
                                cluster.InfectedIntCount, healthyfile, infectedfile, OutType.Int, outList),
                            cluster.Int0.Partition, cluster.Int1.Partition, OutType.Int);
                        break;
                    case ClusterType.G3T:
                        dataOut = G3(
                            GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition, cluster.HealthyTenCount,
                                cluster.InfectedTenCount, healthyfile, infectedfile, OutType.Ten, outList),
                            cluster.Ten0.Partition, cluster.Ten1.Partition, OutType.Ten);
                        break;
                    case ClusterType.G3V:
                        dataOut = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                                cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile,
                                OutType.Vat, outList), cluster.Vat0.Partition, cluster.Vat1.Partition, OutType.Vat);
                        break;
                    case ClusterType.G4I:
                        dataOut = G4(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile, OutType.Int,
                            outList));
                        break;
                    case ClusterType.G4T:
                        dataOut = G4(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyTenCount, cluster.InfectedTenCount, healthyfile, infectedfile, OutType.Ten,
                            outList));
                        break;
                    case ClusterType.G4V:
                        dataOut = G4(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile, OutType.Vat,
                            outList));
                        break;

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //                                            start of G13 - G25                                               //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    case ClusterType.G13:
                        d1 = G1(GroupInitializer(cluster.Vat0.Partition,
                            cluster.Vat1.Partition, cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G2(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition, cluster.HealthyIntCount,
                                cluster.InfectedIntCount, healthyfile, infectedfile, OutType.Int, outList),
                            cluster.Int0.Partition, cluster.Int1.Partition, OutType.Int);
                        d3 = G2(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition, cluster.HealthyVatCount,
                                cluster.InfectedVatCount, healthyfile, infectedfile, OutType.Vat, outList),
                            cluster.Vat0.Partition, cluster.Vat1.Partition, OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G14:
                        d1 = G1(GroupInitializer(cluster.Vat0.Partition,
                            cluster.Vat1.Partition, cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G2(
                            GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition, cluster.HealthyTenCount,
                                cluster.InfectedTenCount, healthyfile, infectedfile, OutType.Ten, outList),
                            cluster.Ten0.Partition, cluster.Ten1.Partition, OutType.Ten);
                        d3 = G2(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition, cluster.HealthyIntCount,
                                cluster.InfectedIntCount, healthyfile, infectedfile, OutType.Int, outList),
                            cluster.Int0.Partition, cluster.Int1.Partition, OutType.Int);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G15:
                        d1 = G1(GroupInitializer(cluster.Ten0.Partition,
                            cluster.Ten0.Partition, cluster.HealthyTenCount, cluster.InfectedTenCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G2(
                            GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition, cluster.HealthyTenCount,
                                cluster.InfectedTenCount, healthyfile, infectedfile, OutType.Ten, outList),
                            cluster.Ten0.Partition, cluster.Ten1.Partition, OutType.Ten);
                        d3 = G2(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition, cluster.HealthyIntCount,
                                cluster.InfectedIntCount, healthyfile, infectedfile, OutType.Int, outList),
                            cluster.Int0.Partition, cluster.Int1.Partition, OutType.Int);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G16:
                        d1 = G1(GroupInitializer(cluster.Int0.Partition,
                            cluster.Int0.Partition, cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d2 = G2(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition, cluster.HealthyIntCount,
                                cluster.InfectedIntCount, healthyfile, infectedfile, OutType.Int, outList),
                            cluster.Int0.Partition, cluster.Int1.Partition, OutType.Int);
                        d3 = G2(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition, cluster.HealthyVatCount,
                                cluster.InfectedVatCount, healthyfile, infectedfile, OutType.Vat, outList),
                            cluster.Vat0.Partition, cluster.Vat1.Partition, OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G17:
                        d1 = G1(GroupInitializer(cluster.Vat0.Partition,
                            cluster.Vat1.Partition, cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                                cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile,
                                OutType.Int, outList), cluster.Int0.Partition, cluster.Int1.Partition,
                            OutType.Int);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                                cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile,
                                OutType.Vat, outList), cluster.Vat0.Partition, cluster.Vat1.Partition,
                            OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G18:
                        d1 = G1(GroupInitializer(cluster.Int0.Partition,
                            cluster.Int1.Partition, cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                                cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile,
                                OutType.Int, outList), cluster.Int0.Partition, cluster.Int1.Partition,
                            OutType.Int);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                                cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile,
                                OutType.Vat, outList), cluster.Vat0.Partition, cluster.Vat1.Partition,
                            OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G19:
                        d1 = G1(GroupInitializer(cluster.Ten0.Partition,
                            cluster.Ten1.Partition, cluster.HealthyTenCount, cluster.InfectedTenCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                                cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile,
                                OutType.Int, outList), cluster.Int0.Partition, cluster.Int1.Partition,
                            OutType.Int);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                                cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile,
                                OutType.Vat, outList), cluster.Vat0.Partition, cluster.Vat1.Partition,
                            OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G20:
                        d1 = G1(GroupInitializer(cluster.Ten0.Partition,
                            cluster.Ten1.Partition, cluster.HealthyTenCount, cluster.InfectedTenCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                                cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile,
                                OutType.Int, outList), cluster.Int0.Partition, cluster.Int1.Partition,
                            OutType.Int);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                                cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile,
                                OutType.Vat, outList), cluster.Vat0.Partition, cluster.Vat1.Partition,
                            OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G21:
                        d1 = G4(GroupInitializer(cluster.Vat0.Partition,
                            cluster.Vat1.Partition, cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                                cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile,
                                OutType.Int, outList), cluster.Int0.Partition, cluster.Int1.Partition,
                            OutType.Int);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                                cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile,
                                OutType.Vat, outList), cluster.Vat0.Partition, cluster.Vat1.Partition,
                            OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G22:
                        d1 = G4(GroupInitializer(cluster.Int0.Partition,
                            cluster.Int1.Partition, cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                                cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile,
                                OutType.Int, outList), cluster.Int0.Partition, cluster.Int1.Partition,
                            OutType.Int);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                                cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile,
                                OutType.Vat, outList), cluster.Vat0.Partition, cluster.Vat1.Partition,
                            OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G23:
                        d1 = G4(GroupInitializer(cluster.Ten0.Partition,
                            cluster.Ten1.Partition, cluster.HealthyTenCount, cluster.InfectedTenCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                                cluster.HealthyIntCount, cluster.InfectedIntCount, healthyfile, infectedfile,
                                OutType.Int, outList), cluster.Int0.Partition, cluster.Int1.Partition,
                            OutType.Int);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                                cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile, infectedfile,
                                OutType.Vat, outList), cluster.Vat0.Partition, cluster.Vat1.Partition,
                            OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G24:
                        d1 = G4(GroupInitializer(cluster.Ten0.Partition,
                            cluster.Ten1.Partition, cluster.HealthyTenCount, cluster.InfectedTenCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                                cluster.HealthyTenCount, cluster.InfectedTenCount, healthyfile, infectedfile,
                                OutType.Ten, outList), cluster.Ten0.Partition, cluster.Ten1.Partition,
                            OutType.Ten);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition, cluster.HealthyVatCount,
                                cluster.InfectedVatCount, healthyfile, infectedfile, OutType.Vat, outList),
                            cluster.Vat0.Partition, cluster.Vat1.Partition, OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G25:
                        d1 = G4(GroupInitializer(cluster.Vat0.Partition,
                            cluster.Vat1.Partition, cluster.HealthyVatCount, cluster.InfectedVatCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G3(
                            GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition, cluster.HealthyTenCount,
                                cluster.InfectedTenCount, healthyfile, infectedfile, OutType.Ten, outList),
                            cluster.Ten0.Partition, cluster.Ten1.Partition, OutType.Ten);
                        d3 = G3(
                            GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition, cluster.HealthyVatCount,
                                cluster.InfectedVatCount, healthyfile, infectedfile, OutType.Vat, outList),
                            cluster.Vat0.Partition, cluster.Vat1.Partition, OutType.Vat);
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;
                }
            }

            return dataOut;
        }

        private static List<List<DataOutStruct>> GroupInitializer(Partition healthyPart, Partition infectedPart,
            int[] healthyCount, int[] infectedCount, String healthyfile,
            String infectedfile, OutType type, List<List<DataOutStruct>> outList)
        {
            String fileEnd = "";
            switch (type)
            {
                case OutType.Vat:
                    fileEnd = "_VAT.csv";
                    break;
                case OutType.Int:
                    fileEnd = "_INT.csv";
                    break;
                case OutType.Ten:
                    fileEnd = "_TEN.csv";
                    break;
            }

            var healthyGroup = GroupNumberInitializer(healthyPart, healthyCount, healthyfile, fileEnd, type);
            // Infected Group
            var infectedGroup = GroupNumberInitializer(infectedPart, infectedCount, infectedfile, fileEnd, type);
            outList.Add(healthyGroup);
            outList.Add(infectedGroup);
            return outList;
        }

        private static GeneralCluster ReturnClusterAndPartition(LightWeightGraph healthy,
            LightWeightGraph infected, int healthyClusters, int infectedClusters, String healthyfile,
            String infectedfile)
        {
            GeneralCluster cluster = new GeneralCluster();
            cluster.Int0.Cluster =
                new HIntegrityClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
            cluster.Int0.Partition = cluster.Int0.Cluster.GetPartition();
            cluster.Int0.Partition.SavePartition(healthyfile + "_INT.cluster", healthyfile + ".graph");
            cluster.Int1.Cluster =
                new HIntegrityClust(infected, infectedClusters + 1, false, 1, 0, false, false);
            cluster.Int1.Partition = cluster.Int1.Cluster.GetPartition();
            cluster.Int1.Partition.SavePartition(infectedfile + "_INT.cluster", infectedfile + ".graph");
            cluster.HealthyIntCount = new int[cluster.Int0.Partition.DataCount];
            cluster.InfectedIntCount = new int[cluster.Int1.Partition.DataCount];
            cluster.Ten0.Cluster =
                new HTenacityClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
            cluster.Ten0.Partition = cluster.Ten0.Cluster.GetPartition();
            cluster.Ten0.Partition.SavePartition(healthyfile + "_TEN.cluster", healthyfile + ".graph");
            cluster.Ten1.Cluster =
                new HTenacityClust(infected, infectedClusters + 1, false, 1, 0, false, false);
            cluster.Ten1.Partition = cluster.Ten1.Cluster.GetPartition();
            cluster.Ten1.Partition.SavePartition(infectedfile + "_TEN.cluster", infectedfile + ".graph");
            cluster.HealthyTenCount = new int[cluster.Ten0.Partition.DataCount];
            cluster.InfectedTenCount = new int[cluster.Ten1.Partition.DataCount];
            cluster.Vat0.Cluster = new HVATClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
            cluster.Vat0.Partition = cluster.Vat0.Cluster.GetPartition();
            cluster.Vat0.Partition.SavePartition(healthyfile + "_VAT.cluster", healthyfile + ".graph");
            cluster.Vat1.Cluster =
                new HVATClust(infected, infectedClusters + 1, false, 1, 0, false, false);
            cluster.Vat1.Partition = cluster.Vat1.Cluster.GetPartition();
            cluster.Vat1.Partition.SavePartition(infectedfile + "_VAT.cluster", infectedfile + ".graph");
            cluster.HealthyVatCount = new int[cluster.Vat0.Partition.DataCount];
            cluster.InfectedVatCount = new int[cluster.Vat1.Partition.DataCount];

            return cluster;
        }

        private static List<DataOutStruct> GroupNumberInitializer(Partition p, int[] cluster, String fileName,
            String fileEnd,
            OutType type)
        {
            List<DataOutStruct> dataOut = new List<DataOutStruct>();

            for (int i = 0; i < p.DataCount; i++)
            {
                cluster[i] = -1;
            }

            for (int i = 0; i < p.Clusters.Count(); i++)
            {
                for (int j = 0; j < p.Clusters[i].Points.Count(); j++)
                {
                    cluster[p.Clusters[i].Points[j].Id] = p.Clusters[i].Points[j].ClusterId;
                }
            }

            for (int i = 0; i < p.DataCount; i++)
            {
                DataOutStruct outObj = new DataOutStruct();
                outObj.Bacteria = p.Graph.Nodes[i].sharedName;
                outObj.ClusterType = type.ToString();
                if (cluster[i] != -1)
                {
                    outObj.GroupNum = cluster[i].ToString();
                }
                else
                {
                    outObj.GroupNum = "N/A";
                }

                dataOut.Add(outObj);
            }

            using (StreamWriter sw = new StreamWriter(fileName + fileEnd))

                for (int i = 0; i < dataOut.Count(); i++)
                {
                    {
                        sw.WriteLine(dataOut[i].Bacteria + "," + dataOut[i].GroupNum + "," + dataOut[i].ClusterType);
                    }
                }

            return dataOut;
        }


        /// <summary>
        /// G1 finds all matching gml clusters with "N/A"
        /// </summary>
        public static List<DataOutStruct> G1(List<List<DataOutStruct>> data)
        {
            List<List<DataOutStruct>> filteredList = new List<List<DataOutStruct>>();
            foreach (List<DataOutStruct> dataSet in data)
            {
                List<DataOutStruct> newList = dataSet.Where(x => x.GroupNum.Equals("N/A")).ToList();
                filteredList.Add(newList);
            }

            List<DataOutStruct> outList = filteredList[0].Intersect(filteredList[1]).OrderBy(x => x.Bacteria).Distinct()
                .ToList();
            return outList;
        }

        /// <summary>
        /// G2 finds all unique maching clusters
        /// </summary>
        public static List<DataOutStruct> G2(List<List<DataOutStruct>> initList, Partition healthyPartition,
            Partition infectedPartition, OutType type)
        {
            List<DataOutStruct> healthy = new List<DataOutStruct>();
            List<DataOutStruct> infected = new List<DataOutStruct>();
            List<DataOutStruct> dataOut = new List<DataOutStruct>();

            foreach (Cluster cluster in healthyPartition.Clusters)
            {
                if (cluster.Points.Count == 1)
                {
                    DataOutStruct singleNode;
                    singleNode.Bacteria = healthyPartition.Graph.Nodes[cluster.Points[0].Id].sharedName;
                    singleNode.ClusterType = type.ToString();
                    singleNode.GroupNum = "";
                    healthy.Add(singleNode);
                }
            }

            foreach (Cluster cluster in infectedPartition.Clusters)
            {
                if (cluster.Points.Count == 1)
                {
                    DataOutStruct singleNode;
                    singleNode.Bacteria = infectedPartition.Graph.Nodes[cluster.Points[0].Id].sharedName;
                    singleNode.ClusterType = type.ToString();
                    singleNode.GroupNum = "";
                    infected.Add(singleNode);
                }
            }
            List<String> commonHealthy = healthy.Select(x => x.Bacteria).Intersect(initList[1].Select(i => i.Bacteria)).ToList();
            List<String> commonInfected = infected.Select(x => x.Bacteria).Intersect(initList[0].Select(i => i.Bacteria)).ToList();
            infected = infected.Where(x => commonInfected.Any(n => n.Equals(x.Bacteria))).ToList();
            healthy = healthy.Where(x => commonHealthy.Any(n => n.Equals(x.Bacteria))).ToList();
            dataOut = healthy.Intersect(infected).OrderBy(x => x.Bacteria).Distinct().ToList();
            return dataOut;
        }


        /// <summary>
        /// G3 finds all unique singular group numbers that are  
        /// </summary>
        public static List<DataOutStruct> G3(List<List<DataOutStruct>> initList, Partition healthyPartition,
            Partition infectedPartition, OutType type)
        {
            List<DataOutStruct> healthy = new List<DataOutStruct>();
            List<DataOutStruct> infected = new List<DataOutStruct>();
            List<DataOutStruct> healthyList = new List<DataOutStruct>();
            List<DataOutStruct> infectedList = new List<DataOutStruct>();

            foreach (Cluster cluster in healthyPartition.Clusters)
            {
                if (cluster.Points.Count == 1)
                {
                    DataOutStruct singleNode;
                    singleNode.Bacteria = healthyPartition.Graph.Nodes[cluster.Points[0].Id].sharedName;
                    singleNode.ClusterType = type.ToString();
                    singleNode.GroupNum = "";
                    healthy.Add(singleNode);
                }
            }

            foreach (Cluster cluster in infectedPartition.Clusters)
            {
                if (cluster.Points.Count == 1)
                {
                    DataOutStruct singleNode;
                    singleNode.Bacteria = infectedPartition.Graph.Nodes[cluster.Points[0].Id].sharedName;
                    singleNode.ClusterType = type.ToString();
                    singleNode.GroupNum = "";
                    infected.Add(singleNode);
                }
            }

            List<String> commonHealthy =
                healthy.Select(x => x.Bacteria).Intersect(initList[1].Select(i => i.Bacteria)).ToList();
            List<String> commonInfected =
                infected.Select(x => x.Bacteria).Intersect(initList[0].Select(i => i.Bacteria)).ToList();
            infectedList = infected.Except(healthy).Distinct().Where(x => commonInfected.Any(n => n.Equals(x.Bacteria)))
                .ToList();
            healthyList = healthy.Except(infected).Distinct().Where(x => commonHealthy.Any(n => n.Equals(x.Bacteria)))
                .ToList();
            List<DataOutStruct> outList = healthyList.Union(infectedList).Distinct().OrderBy(x => x.Bacteria).ToList();
            return outList;
        }

        /// <summary>
        /// G4 finds all bacteria with group number being "N/A" in one file but not the other 
        /// </summary>
        public static List<DataOutStruct> G4(List<List<DataOutStruct>> dataSet)
        {
            List<DataOutStruct> healthyList = new List<DataOutStruct>();
            List<DataOutStruct> infectedList = new List<DataOutStruct>();

            foreach (DataOutStruct healthy in dataSet[0])
            {
                foreach (DataOutStruct infected in dataSet[1])
                {
                    if ((healthy.Bacteria.Equals(infected.Bacteria)) &&
                        (healthy.GroupNum == "N/A" && infected.GroupNum != "N/A"))
                    {
                        healthyList.Add(healthy);
                    }
                }
            }

            foreach (DataOutStruct infected in dataSet[1])
            {
                foreach (DataOutStruct healthy in dataSet[0])
                {
                    if ((infected.Bacteria.Equals(healthy.Bacteria)) &&
                        (infected.GroupNum == "N/A" && healthy.GroupNum != "N/A"))
                    {
                        infectedList.Add(infected);
                    }
                }
            }
            
            List<String> commonHealthy = healthyList.Select(x => x.Bacteria).Intersect(dataSet[1].Select(i => i.Bacteria)).ToList();
            List<String> commonInfected = infectedList.Select(x => x.Bacteria).Intersect(dataSet[0].Select(i => i.Bacteria)).ToList();
            infectedList = infectedList.Distinct().Where(x => commonInfected.Any(n => n.Equals(x.Bacteria))).ToList();
            healthyList = healthyList.Distinct().Where(x => commonHealthy.Any(n => n.Equals(x.Bacteria))).ToList().ToList();
            List<DataOutStruct> outList = healthyList.Union(infectedList).Distinct().OrderBy(x => x.Bacteria).ToList();
            return outList;
        }
<<<<<<< HEAD

=======
>>>>>>> master
    } //-end of class
}