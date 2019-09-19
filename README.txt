Usage: debugNetData.exe <Healthyfile> <Infectedfile> <Output> <Group>
Groups:
G1(V,I,T) - G1 finds all matching .gml clusters with "N/A"
G2(V,I,T) - G2 finds all unique matching singular groups
G3(V,I,T)  - G3 finds all unique singular groups that are in one but not the other
G4(V,I,T)  - G4 finds all bacteria with group number being "N/A" in one file but not the other
G13 - G1V + G2I/G2V
G14 - G1V + G2T/G2I
G15 - G1T + G2T/G2I
G16 - G1I + G2I/G2V
G17 - G1V + G3I/G3V
G18 - G1I + G3I/G3V
G19 - G1T + G3I/G3V
G20 - G1T + G3T/G3I
G21 - G4V + G3I/G3V
G22 - G4I + G3I/G3V
G23 - G4T + G3I/G3V
G24 - G4T + G3T/G3I
G25 - G4V + G3T/G3V
Example:
debugNetData.exe C:\Users\Desktop\healthyfile.gml C:\Users\Desktop\infectedfile.gml C:\Users\Desktop\output.txt G1V
Repo:
https://github.com/xinx9/jm-graphclustering
