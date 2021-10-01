using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace InjectGuidinXMIRootPackage
{
    class Program
    {
        static int Main(string[] args)
        {
            //my.xmi EAPK_704B4B1E-D532-4C19-866D-77B73E4F1435
            Console.WriteLine("InjectGuidinXMIRootPackage is starting");
            try
            {
                if (args.Length != 2)
                {
                    Console.WriteLine($"Wrong number of commandline parameters! (1) e.g.:  PWC.eapx");
                    return -1;
                }

                string filename = args[0];
                if (!(File.Exists(filename)))
                {
                    Console.WriteLine($"{filename} doesn't exist!");
                    return -2;
                }

                string newGuid = args[1];

                string text;
                XDocument srcTree;
                Console.WriteLine($"Modifying {filename} with {newGuid}");

              
                srcTree = new XDocument();
                srcTree = XDocument.Load(filename);
                var query = from c in srcTree.Elements("xmi:XMI") select c; ;//.Elements("Model").Elements("packagedElement") select c;
                foreach (var foo in query)
                {
                    foo.ToString();
                }
                //XmlDocument xmldoc = new XmlDocument();
                //xmldoc.LoadXml(text);
                //XmlElement node1 = xmldoc.SelectSingleNode($"/XMI/Model") as XmlElement;
                //if (node1 != null)
                //{
                //    //
                //}
                //else
                //{
                //    return 8;
                //}



            

                using (var stream = File.Open(filename, FileMode.Truncate))
            {
                // srcTree.WriteTo(stream);
                //text = srcTree.ToString();
                //stream.SetLength(text.Length);

            }


            Console.WriteLine("InjectGuidinXMIRootPackage is finished");
            return 0;
        }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured: {ex.Message}");
                return -9;
            }
        }
    }
}
