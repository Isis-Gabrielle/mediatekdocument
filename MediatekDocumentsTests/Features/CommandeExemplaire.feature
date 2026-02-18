Feature: Commandes Exemplaires

Scenario: Suivi d'une commande
    Given Nouvelle commande id "0002" date de commande "2026-02-07" montant 55
    And La commande a 55 exemplaires du livre id "00017"
    And Nouveau statut id "00002" libelle "Livrée"
    When commande de document instanciee
    Then Le resultat du suivi est "Livrée"

Scenario: Etat d'un exemplaire
    Given Un exemplaire numero 2 achete le "2026-02-08" pour le document "00017"
    And Nouveau etat id "00003" libelle "Détérioré"
    When exemplaire instanciee
    Then Le resultat de l'etat est "Détérioré"