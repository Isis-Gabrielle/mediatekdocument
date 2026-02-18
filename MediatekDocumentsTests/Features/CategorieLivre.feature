Feature: Categorie Livre

Scenario: Libellé d'une catégorie
    Given Nouvelle categorie avec id "0001" et libelle "Horreur"
    When Le libelle est recupere
    Then le resultat est "Horreur"

Scenario: Libellé de plusieurs catégories d'un livre
    Given Nouveau livre avec id "58", titre "livre romantique", ISBN "574954", auteur "Christian"
    And Nouvelle categorie Genre avec id "G0001" et libelle "Romance"
    And Nouvelle categorie Public avec id "P0001" et libelle "Ados"
    And Nouvelle categorie Rayon avec id "R0001" et libelle "Roman"
    When Le libelle est recupere pour chaque categorie
    Then Genre = "Romance", Public = "Ados", Rayon = "Roman"