using Reqnroll;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaTekDocuments.model;
using System;
using System.Globalization;

[Binding]
public class CategorieLivreStepsDefinitions
{    
    // Scénario 1
    private Categorie _categorieTest;
    private string _resultat;

    [Given(@"Nouvelle categorie avec id ""(.*)"" et libelle ""(.*)""")]
    public void GivenCategorie(string id, string libelle)
        => _categorieTest = new Categorie(id, libelle);

    [When(@"Le libelle est recupere")]
    public void WhenLibelle()
    {
        _resultat = _categorieTest.ToString();
    }

    [Then(@"le resultat est ""(.*)""")]
    public void Then(string libelle)
    {
        Assert.AreEqual(libelle, _resultat,
            "La méthode ToString() aurait dû retourner \"Horreur\".");
    }

    // Scénario 2
    private string _id, _titre, _isbn, _auteur;
    private Genre _genreTest;
    private Public _publicTest;
    private Rayon _rayonTest;

    private Livre _livreTest;
    [Given(@"Nouveau livre avec id ""(.*)"", titre ""(.*)"", ISBN ""(.*)"", auteur ""(.*)""")]
    public void GivenInfosLivre(string id, string titre, string isbn, string auteur)
    {
        _id = id;
        _titre = titre;
        _isbn = isbn;
        _auteur = auteur;
    }

    [Given(@"Nouvelle categorie Genre avec id ""(.*)"" et libelle ""(.*)""")]
    public void GivenGenre(string id, string libelle) => _genreTest = new Genre(id, libelle);

    [Given(@"Nouvelle categorie Public avec id ""(.*)"" et libelle ""(.*)""")]
    public void GivenPublic(string id, string libelle) => _publicTest = new Public(id, libelle);

    [Given(@"Nouvelle categorie Rayon avec id ""(.*)"" et libelle ""(.*)""")]
    public void GivenRayon(string id, string libelle) => _rayonTest = new Rayon(id, libelle);

    [When(@"Le libelle est recupere pour chaque categorie")]
    public void WhenAssemblageEtRecuperation()
    {
        _livreTest = new Livre(_id, _titre, "", _isbn, _auteur, "",
            _genreTest.Id, _genreTest.Libelle,
            _publicTest.Id, _publicTest.Libelle,
            _rayonTest.Id, _rayonTest.Libelle);
    }

    [Then(@"Genre = ""(.*)"", Public = ""(.*)"", Rayon = ""(.*)""")]
    public void ThenVerifViaLivre(string libelleGenre, string libellePublic, string libelleRayon)
    {
        Assert.AreEqual(libelleGenre, _livreTest.Genre, "Le test aurait dû retourner Romance");
        Assert.AreEqual(libellePublic, _livreTest.Public, "Le test aurait dû retourner Ados");
        Assert.AreEqual(libelleRayon, _livreTest.Rayon, "Le test aurait dû retourner Roman");
    }
}