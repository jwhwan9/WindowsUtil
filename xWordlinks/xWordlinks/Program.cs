using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace xWordlinks
{
    class Program
    {
        static void Main(string[] args)
        {
            string SrcFilename = "test.docx";
            string DstFilename = "";

            if (args.Length == 1)
            {
                SrcFilename = args[0];
            }

            if (!File.Exists(SrcFilename))
            {
                Console.WriteLine("xWordlinks.exe SrcWord_Filename <DstWord_Filename>");
            }
            else
            {
                FileInfo fi2 = new FileInfo(SrcFilename);
                DstFilename = System.Environment.CurrentDirectory + @"\" + Path.GetFileNameWithoutExtension(fi2.Name) + "_clean" + fi2.Extension;

                File.Copy(SrcFilename, DstFilename, true);
                RemoveHyperlinksInWord(DstFilename);

            }
        }

        static void RemoveHyperlinksInWord(string wordFile)
        {
            // Get the Word application object.
            Word._Application word_app = new Word.Application();

            // Make Word visible (optional).
            word_app.Visible = true;

            // Open the Word document.
            object missing = Type.Missing;
            object filename = wordFile;
            object confirm_conversions = false;
            object read_only = false;
            object add_to_recent_files = false;
            object format = 0;
            Word._Document word_doc =
                word_app.Documents.Open(ref filename,
                    ref confirm_conversions,
                    ref read_only, ref add_to_recent_files,
                    ref missing, ref missing, ref missing, ref missing,
                    ref missing, ref format, ref missing, ref missing,
                    ref missing, ref missing, ref missing, ref missing);

            // Remove the hyperlinks.
            object index = 1;
            int counts = 1;
            while (word_doc.Hyperlinks.Count > 0)
            {
                var myLinks = word_doc.Hyperlinks.get_Item(ref index);
                Console.WriteLine("{0}  ,{1},{2}", counts++, myLinks.TextToDisplay, myLinks.Address);
                myLinks.Delete();
            }

            // Save and close the document without prompting.
            object save_changes = true;
            word_doc.Close(ref save_changes, ref missing, ref missing);

            // Close the word application.
            word_app.Quit(ref save_changes, ref missing, ref missing);
        }
    }
}
