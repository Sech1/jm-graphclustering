using NetMining.ClusteringAlgo;
using NetMining.Graphs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace debugNetData
{
    class Program
    {
        public enum OutType
        {
            Ten,
            Vat,
            Int
        }

        static void Main ( string[] args )
        {
            String workingDir = Directory.GetCurrentDirectory();
            String datapath = workingDir + "/Data";
            String[] filepaths = Directory.GetFiles( datapath );

            if ( !Directory.Exists( datapath ) )
            {
                Directory.CreateDirectory( datapath );
                Console.WriteLine( "Please move <healthyfile> and/or <infectedfile> to: " + datapath );
                Environment.Exit( 0 );
            }
            if ( args.Length == 0 )
            {
                System.Console.WriteLine(
                    "Usage: Program.cs <Healthyfile> <Infectedfile> <clusterType> " );
                Environment.Exit( 0 );
            }

            // AUTOMATING IBD 
            // We need both a healthy network and an IBD network
            // COMMAND LINE: clusteringanalysis.exe healthyNet infectedNet VATorINTorTEN  


            //convert from gml to graph
            String Healthyfile = $"{workingDir}//Data//{args[ 0 ]}";
            String Infectedfile = $"{workingDir}//Data//{args[ 1 ]}";

            LightWeightGraph healthy = LightWeightGraph.GetGraphFromGML( $"{Healthyfile}" );
            LightWeightGraph infected = LightWeightGraph.GetGraphFromGML( $"{Infectedfile}" );
            Healthyfile = Healthyfile.Split( '.' )[ 0 ];
            Infectedfile = Infectedfile.Split( '.' )[ 0 ];
            healthy.SaveGraph( Healthyfile + ".graph" );
            infected.SaveGraph( Infectedfile + ".graph" );
            // Makes a list of what the nodes reference
            using ( StreamWriter sw = new StreamWriter( Healthyfile + ".txt" , true ) )
            {
                for ( int i = 0; i < healthy.Nodes.Length; i++ )
                {
                    sw.WriteLine( healthy.Nodes[ i ].sharedName );
                }
            }

            using ( StreamWriter sw = new StreamWriter( Infectedfile + ".txt" , true ) )
            {
                for ( int i = 0; i < infected.Nodes.Length; i++ )
                {
                    sw.WriteLine( infected.Nodes[ i ].sharedName );
                }
            }

            //we don't actually know the number of clusters in each graph - we want to cluster for 1 more than we start with
            //so cluster for 1 just to get the file.
            //HVATClust clust1 = new HVATClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);

            HVATClust healthyClust1 = new HVATClust( healthy , 1 , false , 1 , 0 , false , false );
            Partition t1 = healthyClust1.GetPartition();
            int healthyClusters = t1.Clusters.Count;
            HVATClust infectedClust1 = new HVATClust( infected , 1 , false , 1 , 0 , false , false );
            Partition t2 = infectedClust1.GetPartition();
            int infectedClusters = t2.Clusters.Count;

            // Now we know the intital number of clusters, do the actual clustering
            //HVATClust clust1 = new HVATClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);

            // This sees if the input cluster type can be parsed as the Enum, and if so 
            // Uses a switch statement to decide which clustering to run.
            if ( args.Length >= 3 )
            {
                if ( Enum.TryParse<OutType>( args[ 2 ] , ignoreCase: true , out var userOut ) )
                {
                    // Healthy Group
                    List<DataOutStruct> healthyGroup;
                    List<DataOutStruct> infectedGroup;
                    switch ( userOut )
                    {
                        case OutType.Int:
                        HIntegrityClust hclust2 =
                            new HIntegrityClust( healthy , healthyClusters + 1 , false , 1 , 0 , false , false );
                        Partition p2 = hclust2.GetPartition();
                        p2.SavePartition( Healthyfile + "_INT.cluster" , Healthyfile + ".graph" );

                        HIntegrityClust iclust2 =
                            new HIntegrityClust( infected , infectedClusters + 1 , false , 1 , 0 , false , false );
                        Partition p5 = iclust2.GetPartition();
                        p5.SavePartition( Infectedfile + "_INT.cluster" , Infectedfile + ".graph" );

                        int[] clusts2 = new int[ p2.DataCount ];
                        int[] clusts5 = new int[ p5.DataCount ];
                        // Healthy Group
                        healthyGroup = rename( p2 , clusts2 , Healthyfile , "_INT.csv" , userOut );
                        // Infected Group
                        infectedGroup = rename( p5 , clusts5 , Infectedfile , "_INT.csv" , userOut );

                        G1( healthyGroup , infectedGroup );
                        G2( healthyGroup , infectedGroup );

                        break;
                        case OutType.Ten:
                        HTenacityClust hclust3 =
                            new HTenacityClust( healthy , healthyClusters + 1 , false , 1 , 0 , false , false );
                        Partition p3 = hclust3.GetPartition();
                        p3.SavePartition( Healthyfile + "_TEN.cluster" , Healthyfile + ".graph" );

                        HTenacityClust iclust3 =
                            new HTenacityClust( infected , infectedClusters + 1 , false , 1 , 0 , false , false );
                        Partition p6 = iclust3.GetPartition();
                        p6.SavePartition( Infectedfile + "_TEN.cluster" , Infectedfile + ".graph" );

                        int[] clusts3 = new int[ p3.DataCount ];
                        int[] clusts6 = new int[ p6.DataCount ];

                        // Healthy Group
                        healthyGroup = rename( p3 , clusts3 , Healthyfile , "_TEN.csv" , userOut );
                        // Infected Group
                        infectedGroup = rename( p6 , clusts6 , Infectedfile , "_TEN.csv" , userOut );
                        List<DataOutStruct> test1 = healthyGroup;
                        List<DataOutStruct> test2 = infectedGroup;
                        G1( test1 , test2 );
                        G2( healthyGroup , infectedGroup );
                        break;
                        case OutType.Vat:
                        HVATClust hclust1 = new HVATClust( healthy , healthyClusters + 1 , false , 1 , 0 , false , false );
                        Partition p1 = hclust1.GetPartition();
                        p1.SavePartition( Healthyfile + "_VAT.cluster" , Healthyfile + ".graph" );

                        HVATClust iclust1 =
                            new HVATClust( infected , infectedClusters + 1 , false , 1 , 0 , false , false );
                        Partition p4 = iclust1.GetPartition();
                        p4.SavePartition( Infectedfile + "_VAT.cluster" , Infectedfile + ".graph" );

                        int[] clusts1 = new int[ p1.DataCount ];
                        int[] clusts4 = new int[ p4.DataCount ];
                        // Healthy Group
                        healthyGroup = rename( p1 , clusts1 , Healthyfile , "_VAT.csv" , userOut );
                        // Infected Group
                        infectedGroup = rename( p4 , clusts4 , Infectedfile , "_VAT.csv" , userOut );
                        G1( healthyGroup , infectedGroup );
                        G2( healthyGroup , infectedGroup );
                        break;
                    }
                }
                else
                {
                    Console.WriteLine( "Please input a valid output type (VAT, INT, TEN) as the third parameter." );
                }
            }
            else
            {
                Console.WriteLine( "Please enter a valid cluster type (INT, VAT, TEN)." );
            }
        }



        private static List<DataOutStruct> rename ( Partition p , int[] cluster , String FileName , String FileEnd , OutType type )
        {
            List<DataOutStruct> dataOut = new List<DataOutStruct>();

            for ( int i = 0; i < p.DataCount; i++ )
            {
                cluster[ i ] = -1;
            }

            for ( int i = 0; i < p.Clusters.Count(); i++ )
            {
                for ( int j = 0; j < p.Clusters[ i ].Points.Count(); j++ )
                {
                    cluster[ p.Clusters[ i ].Points[ j ].Id ] = p.Clusters[ i ].Points[ j ].ClusterId;
                }
            }

            for ( int i = 0; i < p.DataCount; i++ )
            {
                DataOutStruct outObj = new DataOutStruct();
                outObj.bacteria = p.Graph.Nodes[ i ].sharedName;
                outObj.clusterType = type.ToString();
                if ( cluster[ i ] != -1 )
                {
                    outObj.groupNum = cluster[ i ].ToString();
                }
                else
                {
                    outObj.groupNum = "N/A";
                }

                dataOut.Add( outObj );

            }


            // System.Console.WriteLine( dataOut[i].bacteria );
            using ( StreamWriter sw = new StreamWriter( FileName + "_data.csv" ) )

                for ( int i = 0; i < dataOut.Count(); i++ )
                {
                    {
                        sw.WriteLine( dataOut[ i ].bacteria + "," + dataOut[ i ].groupNum + "," + dataOut[ i ].clusterType );
                    }
                }
            return dataOut;
        }


        /// <summary>
        /// G1 finds all matching gml clusters with "N/A"
        /// </summary>
        public static void G1 ( List<DataOutStruct> healthy , List<DataOutStruct> infected )
        {
            List<string> IBAC = new List<string>();
            List<DataOutStruct> G1Ret = new List<DataOutStruct>();
            for ( int j = 0; j < infected.Count(); j++ )
            {
                IBAC.Add( infected[ j ].bacteria );
            }
            healthy = reduce( healthy , IBAC );
            infected = reduce( infected , IBAC );
            for ( int i = 0; i < healthy.Count(); i++ )
            {
                for ( int j = 0; j < IBAC.Count(); j++ )
                {
                    if ( healthy[ i ].bacteria.Equals( IBAC[ j ] ) )
                    {
                        G1Ret.Add( healthy[ i ] );
                    }
                }
            }
            List<DataOutStruct> hea = reuse( healthy , G1Ret );
            List<DataOutStruct> inf = reuse( infected , G1Ret );
            G1Ret.Clear();
            for ( int i = 0; i < hea.Count(); i++ )
            {
                for ( int j = 0; j < inf.Count(); j++ )
                {
                    if ( hea[ i ].bacteria.Equals( inf[ j ].bacteria ) )
                    {
                        if ( hea[ i ].groupNum.Equals( inf[ j ].groupNum ) && hea[ i ].groupNum.Equals( "N/A" ) )
                        {
                            G1Ret.Add( hea[ i ] );
                        }
                    }
                }
            }
            using ( StreamWriter recycle = new StreamWriter( "./Data/G1.csv" ) )
            {
                for ( int i = 0; i < G1Ret.Count(); i++ )
                    recycle.WriteLine( G1Ret[ i ].bacteria + "," + G1Ret[ i ].groupNum );
            }
        }

        public static List<DataOutStruct> reduce ( List<DataOutStruct> dos , List<string> bac )
        {
            for ( int i = 0; i < dos.Count(); i++ )
            {
                if ( !bac.Contains( dos[ i ].bacteria ) )
                {
                    dos.Remove( dos[ i ] );
                }
            }
            return dos;
        }

        public static List<DataOutStruct> reuse ( List<DataOutStruct> dos , List<DataOutStruct> G1R )
        {
            List<DataOutStruct> temp = new List<DataOutStruct>();
            List<DataOutStruct> tG1R = G1R;

            for ( int i = 0; i < dos.Count(); i++ )
            {
                for ( int j = 0; j < tG1R.Count(); j++ )
                {
                    if ( dos[ i ].bacteria.Equals( tG1R[ j ].bacteria ) )
                    {
                        temp.Add( dos[ i ] );
                    }
                }
            }
            return temp;
        }

        /// <summary>
        /// G2 finds all unique maching clusters
        /// </summary>
        public static void G2 ( List<DataOutStruct> healthy , List<DataOutStruct> infected )
        {
            healthy = removeDuplicate( healthy );
            infected = removeDuplicate( infected );
            List<DataOutStruct> holdme = new List<DataOutStruct>();
            for ( int i = 0; i < healthy.Count(); i++ )
            {
                for ( int j = 0; j < infected.Count(); j++ )
                {
                    if ( healthy[ i ].bacteria.Equals( infected[ j ].bacteria ) )
                    {
                        if ( healthy[ i ].groupNum.Equals( infected[ j ].groupNum ) )
                        {
                            holdme.Add( healthy[ i ] );
                        }
                    }
                }
            }
            using ( StreamWriter sw = new StreamWriter( "./Data/G2.csv" ) )
            {
                for ( int i = 0; i < holdme.Count(); i++ )
                {
                    sw.WriteLine( holdme[ i ].bacteria + "," + holdme[ i ].groupNum );
                }
            }
        }

        public static List<DataOutStruct> removeDuplicate ( List<DataOutStruct> a )
        {
            List<DataOutStruct> del = a.GroupBy( x => x.groupNum )
                                       .Where( x => x.Count() == 1 )
                                       .Select( x => x.FirstOrDefault() ).ToList();
            return del;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void G3 ( List<DataOutStruct> healthy , List<DataOutStruct> infected )
        {

        }
    }
}
