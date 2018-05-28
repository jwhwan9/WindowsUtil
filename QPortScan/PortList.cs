using System.Collections.Generic;

namespace MultiPortScan
{
    class PortList
    {
        private int start;
        private int stop;
        private int ports;
        List<int> additionPort = new List<int>();
        private int additionStop = 0;

        public PortList(int starts, int stops)
        {
            start = starts;
            stop = stops;
            ports = start;
        }

        public void addPorts(int[] addPorts){
            for (int i = 0; i < addPorts.Length; i++)
            {
                additionPort.Add(addPorts[i]);
            }
        }

        public bool MorePorts()
        {
            if ((stop - ports) >= 0){
                return true;
            }
            else
            {
                if (additionStop < additionPort.Count)
                {
                    return true;
                }
            }

            return false;
        }

        public int NextPort()
        {
            if (MorePorts())
            {
                if ((stop - ports) >= 0)
                {
                    return ports++;
                }
                else
                {
                    if (additionStop < additionPort.Count)
                    {
                        return (int)additionPort[additionStop++];
                    }
                }
            }
            return -1;
        }
    }
}
