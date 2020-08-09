The files in this folder are examples of templates, graphs and JSON file that could be generated or used by the tool

While none of these files are actually used by the tool, some of them are copies of files that the tool does use

HelpInfo.json : The help and faq section in the app is loaded from this file
HelpInfo.ngmap : The graph that exports HelpInfo.json
HelpInfo.tmplt : The template that HelpInfo.ngmap is made with
HelpInfoTemplate.ngmap : The graph that exports HelpInfo.tmplt

TEMPLATE.tmplt : The base template that the tool opens with. It is meant for building out template files that shape data to your needs.
TEMPLATE.ngmap : The graph that generates TEMPLATE.tmplt. Since all template files are JSONs and this tool is menat to generate JSON file 
		I made the master template in the tool as well. This should give you a good example of a template for a lot of complexity.

The last set of files is just a bit more of a real world example. Missions is designed after a generic mission system I used for game dev.

Missions.Json : The file my game would load
Missions.ngmap : Graph file for the above
Missions.tmplt : Template file for the above
MissionsTemplate.ngmap : Graph that generates the mission template

It should be noted that the *.tmplt file actually gets stored inside of the .ngmap file. This means that the only file needed to load a 
graph(or pass it between teams) is the .ngmap file. Keeping the *.tmplt file is advised, as well as the graph file that generates it, 
but neither is needed to make changes to the data itself.

