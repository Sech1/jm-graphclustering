Usage: debugNetData.exe <Healthyfile> <Infectedfile> <output> <Group>


Groups:
G1 - G1 finds all matching gml clusters with "N/A"
G2 - G2 finds all unique maching singular groups 
G3 - G3 finds all unique singular groups that are in one but not the other
G4 - G4 finds all bacteria with group number being "N/A" in one file but not the other 
G13/G25 - finds combinations of G1-G4


Example:
debugNetData.exe C:\Users\admin\Desktop\healthyfile.gml C:\Users\admin\Desktop\infectedfile.gml C:\Users\admin\Desktop\output.txt G1v


Repo:
https://github.com/xinx9/jm-graphclustering
