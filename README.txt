Usage: debugNetData.exe <Healthyfile> <Infectedfile> <Group>


Groups:
G1 - G1 finds all matching gml clusters with "N/A"
G2 - G2 finds all unique maching singular groups 
G3 - G3 finds all unique singular groups that are in one but not the other
G4 - G4 finds all bacteria with group number being "N/A" in one file but not the other 
G13/G25 - finds combinations of G1-G4


Output:
data outputs to the Data file.
C:\(Working Directory)\Data


Example:
debugNetData.exe C:\Users\Desktop\healthyfile.gml C:\Users\Desktop\infectedfile.gml G1V

debugNetData.exe healthyfile.gml infectedfile.gml G1V

debugNetData.exe healthyfile.gml C:\Users\Desktop\infectedfile.gml G1V


Repo:
https://github.com/xinx9/jm-graphclustering
