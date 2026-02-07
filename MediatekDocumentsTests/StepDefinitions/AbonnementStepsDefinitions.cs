using Reqnroll;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaTekDocuments.model;
using System;
using System.Globalization;

[Binding]
public class AbonnementStepsDefinitions
{
    private DateTime _dateCommande;
    private DateTime _dateFin;
    private bool _resultat;

    private readonly Abonnement _abonnementTest = new Abonnement("0", DateTime.Now, 0, DateTime.Now, "0");

    [Given(@"la date de commande est ""(.*)""")]
    public void GivenCommande(string date)
        => _dateCommande = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

    [Given(@"la date de fin est ""(.*)""")]
    public void GivenFin(string date)
        => _dateFin = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

    [When(@"la date de parution est ""(.*)""")]
    public void WhenParution(string date)
    {
        DateTime dateParution = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        _resultat = _abonnementTest.ParutionDansAbonnement(_dateCommande, _dateFin, dateParution);
    }

    [Then(@"le resultat est vrai")]
    public void ThenVrai() => Assert.IsTrue(_resultat, "Le test aurait dû retourner VRAI");

    [Then(@"le resultat est faux")]
    public void ThenFaux() => Assert.IsFalse(_resultat, "Le test aurait dû retourner FAUX");
}