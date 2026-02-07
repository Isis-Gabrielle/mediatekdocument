Feature: Parution abonnement

 Scenario: Parution dans l'abonnement
    Given la date de commande est "01/01/2024"
    And la date de fin est "31/12/2024"
    When la date de parution est "15/06/2024"
    Then le resultat est vrai

 Scenario: Parution hors abonnement
    Given la date de commande est "01/01/2024"
    And la date de fin est "31/12/2024"
    When la date de parution est "01/02/2025"
    Then le resultat est faux

