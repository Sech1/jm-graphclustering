using NetMining.ClusteringAlgo;

namespace debugNetData
{
    public enum OutType
    {
        Ten,
        Vat,
        Int
    }

    public enum ClusterType
    {
        G1V,
        G1I,
        G1T,
        G2V,
        G2I,
        G2T,
        G3V,
        G3I,
        G3T,
        G4V,
        G4I,
        G4T,
        G13, // G1V + (G2I/G2V)
        G14, // G1V + (G2T/G2I)
        G15, // G1T + (G2T/G2I)
        G16, // G1I + (G2I/G2V)
        G17, // GIV + (G2I/G2V)
        G18, // G1I + (G3I/G3V)
        G19, // G1T + (G3I/G3V)
        G20, // G1T + (G3T/G3I)
        G21, // G4V + (G3I/G3V)
        G22, // G4I + (G3I/G3V)
        G23, // G4T + (G3I/G3V)
        G24, // G4T + (G3T/G3I)
        G25 // G1V + (G3T/G3V)
    }

    public class DataOutStruct
    {
        public string GroupNum { get; set; }
        public string Bacteria { get; set; }
        public string ClusterType { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is DataOutStruct))
            {
                return false;
            }

            DataOutStruct other = (DataOutStruct) obj;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (GroupNum != null ? GroupNum.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Bacteria != null ? Bacteria.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ClusterType != null ? ClusterType.GetHashCode() : 0);
                return hashCode;
            }
        }
        
        public bool Equals(DataOutStruct other)
        {
            return Bacteria == other.Bacteria && ClusterType == other.ClusterType
                                              && GroupNum == other.GroupNum;
        }
    }
    
    public struct IntCluster
    {
        public HIntegrityClust Cluster;
        public Partition Partition;
    }

    public struct TenCluster
    {
        public HTenacityClust Cluster;
        public Partition Partition;
    }

    public struct VatCluster
    {
        public HVATClust Cluster;
        public Partition Partition;
    }

    public struct GeneralCluster
    {
        // 0 is healthy
        // 1 is infected
        public VatCluster Vat0;
        public VatCluster Vat1;
        public TenCluster Ten0;
        public TenCluster Ten1;
        public IntCluster Int0;
        public IntCluster Int1;
        public int[] InfectedVatCount;
        public int[] HealthyVatCount;
        public int[] InfectedIntCount;
        public int[] HealthyIntCount;
        public int[] InfectedTenCount;
        public int[] HealthyTenCount;
    }

    public class idCompare : System.Collections.Generic.IEqualityComparer<DataOutStruct>
    {
        public bool Equals ( DataOutStruct x , DataOutStruct y )
        {
            if ( object.ReferenceEquals( x , y ) )
            {
                return true;
            }
            if ( object.ReferenceEquals( x , null ) ||
                object.ReferenceEquals( y , null ) )
            {
                return false;
            }
            return x.Bacteria == y.Bacteria;
        }

        public int GetHashCode ( DataOutStruct obj )
        {
            if ( obj.Equals(null) )
            {
                return 0;
            }
            return obj.Bacteria.GetHashCode();
        }
    }
}