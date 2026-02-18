using Reqnroll;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaTekDocuments.model;
using System;
using System.Globalization;
using System.Security.Cryptography;

[Binding]
public class CommandeExemplaireStepsDefinitions
    {
            private string _idCom, _idLivre;
            private DateTime _dateCom;
            private int _montant, _nbExemplaires;
            private CommandeDocument _commandeDocTest;
            private Suivi _suiviTest;

            // Scénario 1
            [Given(@"Nouvelle commande id ""(.*)"" date de commande ""(.*)"" montant (.*)")]
            public void GivenInfosBaseCommande(string id, string date, int montant)
            {
                _idCom = id;
                _dateCom = DateTime.Parse(date);
                _montant = montant;
            }

            [Given(@"La commande a (.*) exemplaires du livre id ""(.*)""")]
            public void GivenInfosLivreCommande(int nb, string idLivre)
            {  
                 _nbExemplaires = nb;
                _idLivre = idLivre;
              }

            [Given(@"Nouveau statut id ""(.*)"" libelle ""(.*)""")]
            public void GivenSuivi(string id, string libelle)
            {
                _suiviTest = new Suivi(id, libelle);

             }

            [When(@"commande de document instanciee")]
            public void WhenInstancieCommande()
            {
                 _commandeDocTest = new CommandeDocument(_idCom, _dateCom, _montant, _nbExemplaires, _idLivre, _suiviTest.Id, _suiviTest.Libelle);
            }

            [Then(@"Le resultat du suivi est ""(.*)""")]
            public void ThenVerifSuivi(string libelle)
            {
                Assert.AreEqual(libelle, _commandeDocTest.LibelleSuivi, "Le test aurait dû retourner \"Livrée\"");
            }

            // Scénario 2

            private int _num;
            private DateTime _dateAchat;
            private string _idDoc;
            private Exemplaire _exemplaireTest;
            private Etat _etatTest;

            [Given(@"Un exemplaire numero (.*) achete le ""(.*)"" pour le document ""(.*)""")]
            public void GivenInfosExemplaire(int numero, string date, string idDoc)
            {
                _num = numero;
                _dateAchat = DateTime.Parse(date);
                _idDoc = idDoc;
            }

            [Given(@"Nouveau etat id ""(.*)"" libelle ""(.*)""")]
            public void GivenEtat(string id, string libelle)
            {
                _etatTest = new Etat(id, libelle);
            }

            [When(@"exemplaire instanciee")]
            public void WhenInstancieExemplaire()
            {
                _exemplaireTest = new Exemplaire(_num, _dateAchat, "", _etatTest.Id, _idDoc, _etatTest.Libelle);
            }

            [Then(@"Le resultat de l'etat est ""(.*)""")]
            public void ThenVerifEtat(string libelle)
            {
                Assert.AreEqual(libelle, _exemplaireTest.LibelleEtat, "Le test aurait dû retourner \"Détérioré\"");
            }
        }
    

