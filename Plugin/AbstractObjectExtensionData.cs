using ParkourScugPlugin.RevenantAbilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkourScugPlugin
{
    public class AbstractObjectExtensionData
    {
        public readonly AbstractPhysicalObject abstractObject;

        public AbstractObjectExtensionData(AbstractPhysicalObject obj)
        {
            abstractObject = obj;
        }


        public MechanicalProgram program;
    }
}
