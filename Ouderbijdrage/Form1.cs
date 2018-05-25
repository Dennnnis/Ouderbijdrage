using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ouderbijdrage
{
    public partial class Form1 : Form
    {
        const decimal maximaalBedrag = 150.0m;
        const decimal reductieProcentage = 0.25m;

        Dictionary<string, CKind> kinderen = new Dictionary<string, CKind>();
        List<CPrijs> bon = new List<CPrijs>();
        

        public class CKind
        {
            public string voornaam;
            public string achternaam;
            public DateTime geboortedatum;
            public int leeftijd;

            public new string ToString()
            {
                return "Voornaam: " + voornaam + "\n"
                +      "Achternaam: " + achternaam + "\n"
                +      "Geboortedatum: " + geboortedatum.ToShortDateString() + "\n"
                +      "Leeftijd: " + Convert.ToString(leeftijd) + "\n";   
            }
        }


        class CPrijs
        {
            public string titel = "";
            public decimal bedrag = 0.0m;
            public bool maximaal = false;
            public bool reductie = false;
        }


        public Form1()
        {
            InitializeComponent();
            listBox1.Items.Clear();
            kinderen.Clear();
            BerekenBon();
        }


        private decimal BerekenPrijsVanBon(bool speciaal)
        {
            decimal prijsTotaal = 0.0m;
            foreach(CPrijs prijs in bon)
            {
                prijsTotaal += Afronding(prijs.bedrag);

                if (prijs.maximaal && speciaal)   
                    prijsTotaal = maximaalBedrag;

                if (prijs.reductie && speciaal)
                    prijsTotaal *= Afronding(1.0m - reductieProcentage);
            }
            return prijsTotaal;
        }


        private decimal Afronding(decimal bedrag)
        {
            return Math.Round(bedrag,2); 
        }


        private string EuroTekenEnNull(decimal bedrag)
        {
            string stringBedrag = bedrag.ToString();


            while (stringBedrag.Length < Math.Round(bedrag).ToString().Length + 3)
            {
               stringBedrag += "0";
            }

            stringBedrag = stringBedrag.Insert(0, "€ ");

            return stringBedrag;
        }


        private string SchijfBon()
        {
            decimal prijsTotaal = BerekenPrijsVanBon(false);

            string buffer = DateTime.Today.ToLongDateString() + "\n";
            buffer += "Ouderbijdrage calculatie\n";
            buffer += "Basis school \n";
            buffer += "__________________________________\n\n";

            foreach (CPrijs prijs in bon)
            {
               
                if (prijs.bedrag > 0)
                {
                    buffer += EuroTekenEnNull(Afronding(prijs.bedrag));
                }
                else if (prijs.maximaal)
                {
                    buffer += EuroTekenEnNull(Afronding(150 - prijsTotaal));
                }
                else if (prijs.reductie)
                {
                    buffer += EuroTekenEnNull(Afronding(-(prijsTotaal * reductieProcentage)));
                }

                buffer += "   " + prijs.titel + "\n";

            }
            buffer += "__________________________________\n\n";
            buffer += "Prijs totaal: " + EuroTekenEnNull(Afronding(BerekenPrijsVanBon(true)));

            return buffer;
        }

        //Toevoegen
        private void button1_Click(object sender, EventArgs e)
        {
            if (!kinderen.ContainsKey(textBox1.Text))
            {
                CKind niewKind = new CKind();

                try
                {
                    niewKind.voornaam = textBox1.Text;
                    niewKind.achternaam = textBox2.Text;
                    niewKind.geboortedatum = Convert.ToDateTime(textBox3.Text);
                    niewKind.leeftijd = DateTime.Today.Year - niewKind.geboortedatum.Year;
                }
                catch(System.FormatException)
                {
                    MessageBox.Show("Geen geldige informatie", "Probleem");
                    textBox3.Text = "DD/MM/YYYY";
                    return;
                }
                

                //Als het kind onder de 10 jaar is
                if (niewKind.leeftijd < 10)
                {
                    //Tel hoeveel kinderen er zijn onder 10 jaar
                    int KinderenJongerDan10 = 0;
                    foreach (KeyValuePair<string, CKind> kind in kinderen)
                    {
                        if (kind.Value.leeftijd < 10)
                            KinderenJongerDan10++;
                    }

                    if (KinderenJongerDan10 > 2)
                    {
                        MessageBox.Show("U heeft al 3 kinderen onder 10 jaar", "Probleem");
                        return;
                    }
                }
                //Kind is boven 10 jaar
                else
                {
                    //Tel hoeveel kinderen er zijn boven 10 jaar
                    int KinderenOuderDan10 = 0;
                    foreach (KeyValuePair<string, CKind> kind in kinderen)
                    {
                        if (kind.Value.leeftijd > 10)
                            KinderenOuderDan10++;
                    }
                    if (KinderenOuderDan10 > 1)
                    {
                        MessageBox.Show("U heeft al 2 kinderen boven 10 jaar", "Probleem");
                        return;
                    }
                }

                listBox1.Items.Add(textBox1.Text);
                kinderen.Add(textBox1.Text, niewKind);
            }
            else
            {
                MessageBox.Show("Dit kind heeft u all toegevoegd.","Probleem");
            }
            BerekenBon();
        }

        //Bereken
        private void BerekenBon()
        {
            bon.Clear();
            bon.Add(new CPrijs(){ titel = "Basisbedrag",bedrag = 50.0m});

            foreach (KeyValuePair<string,CKind> kind in kinderen)
            {

                if (kind.Value.leeftijd < 10)
                    bon.Add(new CPrijs() { titel = "Kind, onder 10 jaar (" + kind.Value.voornaam + ")", bedrag = 25.0m });
                else
                    bon.Add(new CPrijs() { titel = "Kind, 10 jaar en ouder (" + kind.Value.voornaam + ")", bedrag = 37.0m });
            }

            if (BerekenPrijsVanBon(false) > maximaalBedrag)
            {
                bon.Add(new CPrijs() { titel = "Totaal prijs limiet van €150.00", maximaal = true });
            }

            if (checkBox2.Checked)
            {
                bon.Add(new CPrijs() { titel = "Eenoudergezin reductie van 25%", reductie = true });
            }

            label4.Text = SchijfBon();
        }

        //Kopieer naar clipboard
        private void button3_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(SchijfBon());
        }

        //Verwijder
        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                kinderen.Remove(listBox1.SelectedItem.ToString());
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
            BerekenBon();
        }

        //Info
        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                MessageBox.Show(kinderen[listBox1.SelectedItem.ToString()].ToString(),"Informatie");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            BerekenBon();
        }
    }
}
