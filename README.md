# MediatekDocuments
Cette application permet de gérer les documents (livres, DVD, revues) d'une médiathèque. Elle a été codée en C# sous Visual Studio 2022. C'est une application de bureau, prévue d'être installée sur plusieurs postes accédant à la même base de données.<br>
L'application exploite une API REST pour accéder à la BDD MySQL. Des explications sont données plus loin, ainsi que le lien de récupération.
L'application se repose sur l'existant dont le dépôt initial se trouve ici et contient les informations sur le code existant:
https://github.com/CNED-SLAM/MediaTekDocuments
## Présentation
L'application a été enrichie. Voici les fonctionnalités qui ont été ajoutées et qui sont opérationnelles :
-Ajout, suppression et modification des documents de la médiathèque (livres, DVD, revues).<br>
<img width="592" height="521" alt="diagramme1" src="https://github.com/user-attachments/assets/e3b88274-ceec-4200-a6c6-280675a14332" />
-Affichage, modification et suppression des exemplaires physiques des documents.<br>
<img width="592" height="521" alt="3 drawio" src="https://github.com/user-attachments/assets/f4c8a495-a571-4d50-a44e-cc3f727c3da9" />
-Affichage, modification, ajout et suppression des commandes/abonnements des documents.<br>
<img width="592" height="521" alt="2 drawio" src="https://github.com/user-attachments/assets/45ede245-9cfd-487b-87aa-dbd43879de2f" />
-Système d'authentification
<img width="479" height="401" alt="4 drawio" src="https://github.com/user-attachments/assets/9ccdcb93-5641-4c3c-9087-b2387baea817" />

<br>L'application comporte plusieurs fenêtres de gestions divisée en plusieurs onglets dont une fenêtre d'authentification.
# Fenêtre de gestion des documents et des exemplaires
## Les différents onglets
### Onglet 1 : Livres
Cet onglet présente la liste des livres, triée par défaut sur le titre, ainsi que la liste des exemplaires de chaque livre.<br>
<img width="974" height="1058" alt="image" src="https://github.com/user-attachments/assets/9dc3dd1b-9df7-4857-bcba-f10a5093a685" />

#### Système de gestion des documents
Les boutons respectifs permettent d'ajouter, modifier ou supprimer les livres, sous certaines conditions.
L'ajout d'un document ne suppose pas forcément une commande, même s'il intervient classiquement au moment
d'une commande. 
La suppression d'un document n'est possible que s'il ne possède aucun exemplaire ni commande.

#### Système de gestion des exemplaires
Dans la partie Exemplaire, il est permis de supprimer chaque exemplaire de la liste, et de modifier son état d'usure via la combobox avant de valider.
<br> Le bouton "Commander un exemplaire" permet d'ouvrir la fenêtre de gestion des commandes.

### Onglet 2 : DVD
Cet onglet présente la liste des DVD, triée par titre.<br>
Le fonctionnement est identique à l'onglet des livres.<br>

### Onglet 3 : Revues
Cet onglet présente la liste des revues, triées par titre.<br>
Le fonctionnement est identique à l'onglet des livres.<br>

# Fenêtre de gestion des commandes
## Les différents onglets
### Onglet 1 : Livres
Cet onglet présente la liste des commandes de chaque livre, avec les informations du livre en question.<br>
<img width="1085" height="780" alt="image" src="https://github.com/user-attachments/assets/b7291099-abbc-4c36-a32e-60508de9d57d" />

#### Système de gestion des commandes
Il est possible de commander un ou plusieurs exemplaires d'un livre et de suivre l'évolution
d'une commande.
Une commande passe par différents stades :
- au moment de son enregistrement, elle est "en cours" ;
- au moment de sa réception, elle est "livrée" ;
- une fois livrée, le paiement est effectué, elle est alors "réglée" ;
- dans le cas où la livraison tarde, la commande est "relancée" ;
- <br>
La fenêtre permet de voir, pour les livres, la liste des commandes et gérer le suivi.
Lorsqu'une commande est "livrée", les exemplaires concernés sont
automatiquement générés dans la BDD, avec un numéro séquentiel par rapport au document concerné.
Une commande ne peut être supprimée que si elle n'est pas encore livrée.

### Onglet 2 : DVD
Cet onglet présente la liste des commandes de DVD.<br>
Le fonctionnement est identique à l'onglet des livres.<br>

### Onglet 3 : Revues
Cet onglet présente la liste des abonnements de revues.<br>
Le fonctionnement est identique à l'onglet des livres, sauf qu'un système d'expiration est présent, et affiche un message automatiquement lors de l'ouverture de la fenêtre.
Ce message affiche la liste des abonnements sur le point d'expirer.<br>
<img width="498" height="191" alt="image" src="https://github.com/user-attachments/assets/1107c221-bb92-4159-b394-1619dd5f91c0" />

# Fenêtre d'authentification
L'application démarre sur une authentification qui permet de déterminer si l'employé est reconnu et son
service d'affectation.
Suivant le service d'appartenance de l'employé, certaines fonctionnalités ne seront pas accessibles. Dans le cas
des employés du service Culture, un message les informe que cette application n'est pas accessible pour eux.
<img width="354" height="290" alt="image" src="https://github.com/user-attachments/assets/e619df8e-5bef-4b63-bbfe-8b8d91bcd681" />

## La base de données
La base de données 'mediatek86 ' est au format MySQL.<br>
Voici sa structure :<br>
<img width="1095" height="1022" alt="image" src="https://github.com/user-attachments/assets/8db2dbfc-806e-417d-8612-95b6fee36e02" />

## L'API REST
L'accès à la BDD se fait à travers une API REST protégée par une authentification basique.<br>
Le code de l'API se trouve ici :<br>
https://github.com/Isis-Gabrielle/api_mediatekdocuments<br>
avec toutes les explications pour l'utiliser (dans le readme).

## Installation de l'application en local
- Télécharger le code et le dézipper puis renommer le dossier en "mediatekdocuments".<br>
- Récupérer et installer l'API REST nécessaire (https://github.com/Isis-Gabrielle/api_mediatekdocuments) ainsi que la base de données (les explications sont données dans le readme correspondant).
- Pour installer l'application en elle-même dans son mode production, l'installeur MediaTekDocumentsInstalleur.msi est disponible dans le projet.
