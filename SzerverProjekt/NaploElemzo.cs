using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SzerverProjekt
{
    public class NaploElemzo
    {
        private List<Szerver> szerverek;
        private List<Hibanaplo> naplok;

        public List<Szerver> Szerverek
        {
            get => szerverek;
            set => szerverek = value;
        }

        public List<Hibanaplo> Naplok
        {
            get => naplok;
            set => naplok = value;
        }

        // Konstruktor: beolvassa a fájlokat és feltölti a listákat
        public NaploElemzo(string szerverFajlUtvonal, string naploFajlUtvonal)
        {
            szerverek = File.ReadAllLines(szerverFajlUtvonal)
                .Select(sor => sor.Split(';'))
                .Select(adatok => new Szerver(
                    int.Parse(adatok[0]),
                    adatok[1],
                    adatok[2],
                    adatok[3]))
                .ToList();

            naplok = File.ReadAllLines(naploFajlUtvonal)
                .Select(sor => sor.Split(';'))
                .Select(adatok => new Hibanaplo(
                    int.Parse(adatok[0]),
                    int.Parse(adatok[1]),
                    DateTime.Parse(adatok[2]),
                    Enum.Parse<HibaSzint>(adatok[3]),
                    adatok[4]))
                .ToList();
        }

        // 1. Add vissza a Budapesten található szerverek nevét egy List<string>-ben!
        public List<string> BudapestSzerverekNeve()
        {
            List<string> budapestSzerverekNeve = szerverek.Where(sz => sz.Helyszin == "Budapest").Select(sz => sz.Nev).ToList();
            return budapestSzerverekNeve;
        }

        // 2. Add vissza az összes "Kritikus" szintű hibanapló szövegét egy listában!
        public List<string> KritikusUzenetekLekerdezese()
        {
            List<string> kritikusUzenetekLekerdezese = naplok.Where(n => n.Szint == HibaSzint.Kritikus).Select(n => n.Uzenet).ToList();
            return kritikusUzenetekLekerdezese;
        }

        // 3. JOIN feladat: Készíts egy listát, amely formázott szövegként tartalmazza a sz nevét és a hibaüzenetet (pl. "SzerverNev: Üzenet")!
        public List<string> SzerverEsHibaLeirasok()
        {
            List<string> szerverEsHibaLeirasok = naplok.Join(szerverek, n => n.SzerverAzonosito, sz => sz.Azonosito, (n, sz) => $"{sz.Nev}: {n.Uzenet}").ToList();
            return szerverEsHibaLeirasok;
        }

        // 4. Számold meg, hogy hány darab "Hiba" vagy "Kritikus" szintű bejegyzés érkezett összesen!
        public int SulyosHibakSzama()
        {
            int sulyosHibakSzama = naplok.Count(n => n.Szint == HibaSzint.Hiba || n.Szint == HibaSzint.Kritikus);
            return sulyosHibakSzama;
        }

        // 5. Van-e olyan szerver, amelyhez egyáltalán nem tartozik hibanapló? (True/False)
        public bool VanNaploNelkuliSzerver()
        {
            bool vanNaploNelkuliSzerver = szerverek.Any(sz => !naplok.Any(n => n.SzerverAzonosito == sz.Azonosito));
            return vanNaploNelkuliSzerver;
        }

        // 6. Csoportosítsd a naplókat hiba szint szerint, és add vissza a darabszámokat egy Dictionary<HibaSzint, int>-ben!
        public Dictionary<HibaSzint, int> NaplokSzintSzerintCsoportositva()
        {
            Dictionary<HibaSzint, int> naplokSzintSzerintCsoportositva = naplok.GroupBy(n => n.Szint).ToDictionary(d => d.Key, d => d.Count());
            return naplokSzintSzerintCsoportositva;
        }

        // 7. JOIN + Aggregáció: Add vissza annak a szervernek a nevét, amelyiken a legtöbb súlyos (Hiba vagy Kritikus) hiba történt!
        public string LegtobbHibasSzerverNeve()
        {
            string legtobbHibasSzerverNeve = naplok.Where(n => n.Szint == HibaSzint.Hiba || n.Szint == HibaSzint.Kritikus)
                .GroupBy(n => n.SzerverAzonosito)
                .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
                .Join(szerverek, g => g.Key, sz => sz.Azonosito, (g, sz) => sz.Nev)
                .First()!;
            return legtobbHibasSzerverNeve;
        }

        // 8. Szűrés időpont alapján: Add vissza a 12:00 óra után történt naplóbejegyzéseket "SzerverNev (Óra:Perc) - Üzenet" formátumban!
        public List<string> DeliNaplokSzerverNevvel()
        {
            List<string> deliNaplokSzerverNevvel = naplok.Where(n => n.Idopont.TimeOfDay > new TimeSpan(12, 0, 0))
                .Join(szerverek, n => n.SzerverAzonosito, sz => sz.Azonosito, (n, sz) => $"{sz.Nev} ({n.Idopont:HH:mm}) - {n.Uzenet}")
                .ToList();
            return deliNaplokSzerverNevvel;
        }

        // 9. Add vissza az összes Linux operációs rendszerű szervert név szerint növekvő sorrendbe rendezve!
        public List<Szerver> LinuxSzerverekRendezve()
        {
            List<Szerver> linuxSzerverekRendezve = szerverek.Where(sz => sz.OperaciosRendszer == "Linux").OrderBy(sz => sz.Nev).ToList();
            return linuxSzerverekRendezve;
        }

        // 10. Add vissza a legelső (időben legkorábbi) hibanapló időpontját!
        public DateTime LegkorabbiNaploIdopont()
        {
            DateTime legkorabbiNaploIdopont = naplok.Min(n => n.Idopont);
            return legkorabbiNaploIdopont;
        }

        // 11. Kapcsold össze a budapesti szervereket a naplóikkal, és gyűjtsd ki az ott történt összes hibaüzenetet!
        public List<string> BudapestiSzerverekUzenetei()
        {
            List<string> budapestiSzerverekUzenetei = szerverek.Where(sz => sz.Helyszin == "Budapest").Join(naplok, sz => sz.Azonosito, n => n.SzerverAzonosito, (sz, n) => n.Uzenet)
                .ToList();
            return budapestiSzerverekUzenetei;
        }

        // 12. Válaszd ki a naplókból a hiba szinteket, és Distinct segítségével add vissza az egyedi előfordulásaikat listában!
        public List<HibaSzint> ElofordultHibaSzintek()
        {
            List<HibaSzint> elofordultHibaSzintek = naplok.Select(n => n.Szint).Distinct().ToList();
            return elofordultHibaSzintek;
        }

        // 13. Csoportosítsd a szervereket az OperaciosRendszer tulajdonságuk alapján!
        public IEnumerable<IGrouping<string, Szerver>> SzerverekOsSzerintCsoportositva()
        {
            IEnumerable<IGrouping<string, Szerver>> szerverekOsSzerintCsoportositva = szerverek.GroupBy(sz => sz.OperaciosRendszer);
            return szerverekOsSzerintCsoportositva;
        }

        // 14. Keresd meg és add vissza a legelső "Kritikus" szintű hibanapló objektumot! Ha nincs, adj vissza null-t.
        public Hibanaplo AzelsoKritikusNaplo()
        {
            Hibanaplo azelsoKritikusNaplo = naplok.Where(n => n.Szint == HibaSzint.Kritikus).OrderBy(n => n.Idopont).First();
            return azelsoKritikusNaplo;
        }

        // 15. Számítsd ki, hogy átlagosan hány hibanapló jut egy szerverre (double értékként)!
        public double AtlagosNaploSzamSzerverenkenet()
        {
            double atlagosNaploSzamSzerverenkenet = (double)naplok.Count / szerverek.Count;
            return atlagosNaploSzamSzerverenkenet;
        }

        // 16. Ellenőrizd, hogy a flotta minden egyes szerverének az azonosítója pozitív szám-e (> 0)!
        public bool MindenSzerverAzonositoPozitiv()
        {
            bool mindenSzerverAzonositoPozitiv = !szerverek.Any(sz => sz.Azonosito < 0);
            return mindenSzerverAzonositoPozitiv;
        }
    }
}