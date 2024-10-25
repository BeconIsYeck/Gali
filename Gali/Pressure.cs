using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Gali; 

class Pressure {
	
}

class PressureNode {
	public Population Population;
	public PressureNode? Next;

	public PressureNode(Population population) {
		Population = population;
	}
}
