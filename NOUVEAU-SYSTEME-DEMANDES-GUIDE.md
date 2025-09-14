# 🍽️ Système de Demandes de Menus - Guide Complet

## 📋 Vue d'Ensemble

Le nouveau système transforme la gestion des articles en un workflow de demandes entre **Admin** et **Fournisseurs**. L'Admin demande des menus et plats, les fournisseurs proposent leurs solutions, et l'Admin peut ensuite affecter les menus acceptés aux vols.

## 🎭 Rôles et Responsabilités

### 👨‍💼 **Administrateur/Manager**

- ✅ Crée des demandes de menus/plats
- ✅ Assigne les demandes aux fournisseurs
- ✅ Accepte ou rejette les propositions des fournisseurs
- ✅ Affecte les menus aux vols
- ✅ Génère automatiquement les BCP depuis les menus
- ✅ Crée automatiquement des comptes fournisseurs

### 🏪 **Fournisseur**

- ✅ Reçoit les demandes qui lui sont assignées
- ✅ Crée des articles et menus pour répondre aux demandes
- ✅ Propose des menus en réponse aux demandes
- ✅ Consulte ses demandes en cours

## 🔄 Workflow Complet

### 1️⃣ **Création d'une Demande (Admin)**

```http
POST /api/demandes
```

L'admin crée une demande avec :

- Titre et description
- Type (Menu, Plat, MenuComplet)
- Liste des plats souhaités
- Date limite
- Commentaires

### 2️⃣ **Assignation à un Fournisseur (Admin)**

```http
POST /api/demandes/{id}/assign
```

L'admin assigne la demande à un fournisseur spécifique.

### 3️⃣ **Réponse du Fournisseur**

```http
POST /api/fournisseurs/demandes/{demandeId}/repondre
```

Le fournisseur propose un menu existant en réponse.

### 4️⃣ **Acceptation de la Proposition (Admin)**

```http
POST /api/demandes/reponses/{reponseId}/accept
```

L'admin accepte la proposition du fournisseur.

### 5️⃣ **Affectation de Menu au Vol (Admin)**

```http
POST /api/vols/{volId}/menus/{menuId}/assign
```

L'admin affecte le menu accepté à un vol spécifique.

### 6️⃣ **Génération Automatique du BCP (Admin)**

```http
POST /api/bonscommandeprevisionnel/generate-from-menus/{volId}
```

Le système génère automatiquement un BCP basé sur les menus assignés au vol.

## 🗄️ Nouveaux Modèles de Données

### **DemandeMenu**

- `Numero` : Identifiant unique (DEM20250913001)
- `Titre` : Titre de la demande
- `Type` : Menu, Plat, ou MenuComplet
- `Status` : EnAttente, EnCours, Completee, Annulee
- `DemandeParUserId` : Admin qui a créé la demande
- `AssigneAFournisseurId` : Fournisseur assigné

### **DemandePlat**

- `NomPlatSouhaite` : Nom du plat demandé
- `TypePlat` : Type d'article (Repas, Boisson, etc.)
- `PrixMaximal` : Budget maximum
- `QuantiteEstimee` : Quantité estimée
- `IsObligatoire` : Si le plat est obligatoire

### **Fournisseur**

- `UserId` : Lié à un utilisateur avec rôle "Fournisseur"
- `CompanyName` : Nom de l'entreprise
- `IsVerified` : Si le fournisseur est vérifié
- `Specialites` : Spécialités du fournisseur

### **DemandeMenuReponse**

- `MenuProposedId` : Menu proposé en réponse
- `PrixTotal` : Prix total proposé
- `IsAcceptedByAdmin` : Si accepté par l'admin

## 🚀 Nouveaux Endpoints API

### **Gestion des Demandes (Admin)**

#### Créer une demande

```http
POST /api/demandes
Content-Type: application/json

{
  "titre": "Menu Vol Paris-Londres",
  "description": "Menu pour vol court courrier",
  "type": "MenuComplet",
  "dateLimite": "2025-09-20T12:00:00Z",
  "demandePlats": [
    {
      "nomPlatSouhaite": "Sandwich Jambon-Beurre",
      "typePlat": "Repas",
      "prixMaximal": 8.50,
      "quantiteEstimee": 150,
      "isObligatoire": true
    }
  ]
}
```

#### Lister les demandes

```http
GET /api/demandes?page=1&pageSize=10&status=EnCours
```

#### Assigner à un fournisseur

```http
POST /api/demandes/{id}/assign
Content-Type: application/json

{
  "fournisseurId": 2,
  "commentairesAdmin": "Urgent - Vol demain",
  "dateLimite": "2025-09-15T18:00:00Z"
}
```

#### Accepter une proposition

```http
POST /api/demandes/reponses/{reponseId}/accept
Content-Type: application/json

"Proposition acceptée - Merci"
```

### **Gestion des Fournisseurs (Admin)**

#### Créer un fournisseur

```http
POST /api/fournisseurs
Content-Type: application/json

{
  "userEmail": "newrest@example.com",
  "userFirstName": "Jean",
  "userLastName": "Dupont",
  "userPassword": "MotDePasseSecurise123!",
  "companyName": "NewRest Catering",
  "address": "123 Rue de la Restauration",
  "phone": "+33123456789",
  "specialites": "Restauration aérienne, menus bio"
}
```

#### Lister les fournisseurs

```http
GET /api/fournisseurs?page=1&pageSize=10
```

#### Vérifier un fournisseur

```http
POST /api/fournisseurs/{id}/verify
```

### **Interface Fournisseur**

#### Mes demandes assignées

```http
GET /api/fournisseurs/mes-demandes?status=EnCours
```

#### Répondre à une demande

```http
POST /api/fournisseurs/demandes/{demandeId}/repondre
Content-Type: application/json

{
  "menuProposedId": 15,
  "nomMenuPropose": "Menu Délice Parisien",
  "descriptionMenuPropose": "Sandwich gourmet + boisson + dessert",
  "prixTotal": 12.50,
  "commentairesFournisseur": "Menu fraîchement préparé"
}
```

### **Affectation de Menus aux Vols**

#### Menus disponibles pour un vol

```http
GET /api/vols/{volId}/menus/available
```

#### Affecter un menu à un vol

```http
POST /api/vols/{volId}/menus/{menuId}/assign
Content-Type: application/json

{
  "typePassager": "Economy",
  "commentaires": "Menu standard pour vol court"
}
```

#### Menus assignés à un vol

```http
GET /api/vols/{volId}/menus
```

#### Générer BCP depuis les menus

```http
POST /api/bonscommandeprevisionnel/generate-from-menus/{volId}
```

#### Statistiques des menus d'un vol

```http
GET /api/bonscommandeprevisionnel/menu-statistics/{volId}
```

## 🔐 Autorisations

### **Endpoints Admin/Manager uniquement :**

- Toute la gestion des demandes (`/api/demandes/*`)
- Création et gestion des fournisseurs (`/api/fournisseurs/*`)
- Affectation des menus aux vols (`/api/vols/*/menus/*`)
- Génération des BCP (`/api/bonscommandeprevisionnel/generate-from-menus/*`)

### **Endpoints Fournisseur uniquement :**

- Ses demandes assignées (`/api/fournisseurs/mes-demandes`)
- Répondre aux demandes (`/api/fournisseurs/demandes/*/repondre`)

## 🎯 Exemples d'Utilisation

### **Scénario 1 : Demande de Menu pour Vol**

1. **Admin crée une demande :**

   ```json
   {
     "titre": "Menu Vol AF1234 Paris-New York",
     "type": "MenuComplet",
     "demandePlats": [
       {
         "nomPlatSouhaite": "Plateau repas Business",
         "typePlat": "Repas",
         "prixMaximal": 25.0,
         "quantiteEstimee": 50
       }
     ]
   }
   ```

2. **Admin assigne à NewRest :**

   ```json
   {
     "fournisseurId": 3,
     "dateLimite": "2025-09-20T12:00:00Z"
   }
   ```

3. **NewRest répond avec un menu :**

   ```json
   {
     "menuProposedId": 42,
     "prixTotal": 23.5,
     "commentairesFournisseur": "Menu gastronomique avec produits locaux"
   }
   ```

4. **Admin accepte et affecte au vol :**

   ```http
   POST /api/demandes/reponses/15/accept
   POST /api/vols/123/menus/42/assign
   ```

5. **Admin génère le BCP automatiquement :**
   ```http
   POST /api/bonscommandeprevisionnel/generate-from-menus/123
   ```

## 🔧 Fonctionnalités Avancées

### **Génération Automatique de BCP**

Le système peut générer automatiquement un Bon de Commande Prévisionnel basé sur :

- Les menus assignés au vol
- Le nombre de passagers estimé
- Les quantités par type de passager (Economy: 80%, Business: 15%, First: 5%)

### **Statistiques des Menus**

```json
{
  "totalMenusAssigned": 3,
  "totalArticles": 12,
  "estimatedTotalCost": 2450.75,
  "menusByPassengerType": {
    "Economy": 2,
    "Business": 1
  },
  "articlesByType": {
    "Repas": 180,
    "Boisson": 200,
    "Consommable": 50
  }
}
```

### **Numérotation Automatique**

- Demandes : `DEM20250913001`
- Plans d'hébergement : `PHAF123420250915001`
- BCP : `BCPAF123420250915001`

## 🎨 Interface Frontend Recommandée

### **Dashboard Admin**

- 📊 Tableau de bord avec statistiques des demandes
- 📝 Liste des demandes en cours
- 👥 Gestion des fournisseurs
- ✈️ Affectation de menus aux vols

### **Interface Fournisseur**

- 📋 Mes demandes assignées
- 🍽️ Mes menus et articles
- 📈 Historique des propositions
- 💬 Communications avec l'admin

## 🚀 Démarrage Rapide

1. **Le rôle Fournisseur est automatiquement créé**
2. **Créer un fournisseur :**
   ```http
   POST /api/fournisseurs
   ```
3. **Créer une demande :**
   ```http
   POST /api/demandes
   ```
4. **Assigner la demande au fournisseur**
5. **Le fournisseur se connecte et répond à la demande**
6. **Admin accepte et affecte le menu au vol**
7. **Génération automatique du BCP**

## 🔄 Migration depuis l'Ancien Système

Les anciens articles et menus restent fonctionnels, mais les nouveaux suivent le workflow de demandes. Pour migrer :

1. Créer des comptes fournisseurs pour les anciens fournisseurs
2. Associer les articles/menus existants aux fournisseurs
3. Utiliser le nouveau workflow pour les futures demandes

---

## 📞 Support

Pour toute question sur l'implémentation, référez-vous aux contrôleurs et services créés :

- `DemandesController.cs` - Gestion des demandes
- `FournisseursController.cs` - Gestion des fournisseurs
- `VolMenusController.cs` - Affectation de menus aux vols
- `MenuService.cs` - Logique métier des menus

Le système est maintenant prêt pour l'intégration frontend ! 🎉
