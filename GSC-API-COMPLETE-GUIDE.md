# 🛫 GSC API - Guide Complet et Workflow

## 📋 Table des Matières

1. [Vue d'ensemble](#vue-densemble)
2. [Architecture du Système](#architecture-du-système)
3. [Workflow Complet GSC](#workflow-complet-gsc)
4. [Endpoints API](#endpoints-api)
5. [Modèles de Données](#modèles-de-données)
6. [Guide de Démarrage](#guide-de-démarrage)
7. [Exemples d'Utilisation](#exemples-dutilisation)

---

## 🎯 Vue d'ensemble

L'API GSC (Gestion de la Sous-traitance du Catering) est une solution complète pour gérer le processus de catering des vols pour Tunisair. Elle couvre l'ensemble du workflow depuis la programmation des vols jusqu'aux rapports budgétaires.

### ✨ Fonctionnalités Principales

- 🛫 **Gestion des Vols** - CRUD complet des vols
- 📋 **Plans d'Hébergement** - Définition des dotations standard
- 🍽️ **Gestion des Menus** - Création et association aux vols
- 📦 **Articles & Inventaire** - Catalogue des produits
- 📝 **Bons de Commande Prévisionnels (BCP)** - Génération automatique
- 📋 **Bons de Livraison (BL)** - Réception et validation
- ⚠️ **Gestion des Écarts** - Rapprochement BCP/BL automatique
- 🏥 **Boîtes Médicales** - Assignation aux vols
- 📁 **Dossiers de Vol** - Consolidation des données
- 📊 **Rapports Budgétaires** - Statistiques et analyses

---

## 🏗️ Architecture du Système

```
┌─────────────────────────────────────────────────────────────┐
│                    FRONTEND (React/Angular)                 │
├─────────────────────────────────────────────────────────────┤
│                       GSC API                               │
├─────────────────────────────────────────────────────────────┤
│  Controllers │  Services  │   Models   │      DTOs         │
├─────────────────────────────────────────────────────────────┤
│                Entity Framework Core                        │
├─────────────────────────────────────────────────────────────┤
│                   SQL Server Database                      │
├─────────────────────────────────────────────────────────────┤
│         Intégrations Externes (ERP, Netline)              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 Workflow Complet GSC

### 1️⃣ Import du Programme de Vol

```
Service Programmation → API Vols → Base de Données
```

### 2️⃣ Affectation du Plan d'Hébergement

```
Moteur Métier → Règles (saison, durée, avion, zone) → Plan d'Hébergement
```

### 3️⃣ Conception des Menus

```
Service Développement → IHM → Menus + Articles → Association au Plan
```

### 4️⃣ Génération du BCP

```
Système → Plan + Menus → BCP → Transmission Traiteur
```

### 5️⃣ Réception du BL

```
Traiteur (NewRest) → BL → Validation → Rapprochement BCP/BL
```

### 6️⃣ Traitement des Écarts

```
Système → Comparaison BCP/BL → Écarts → Traitement Manuel
```

### 7️⃣ Affectation Boîtes Médicales

```
Centre Médical → Sélection Boîtes → Assignation Vol
```

### 8️⃣ Dossier de Vol

```
Système → Consolidation Données → Dossier Complet → Export PDF
```

### 9️⃣ Rapports Budgétaires

```
Système → Agrégation SQL → Statistiques → Rapports
```

### 🔟 Transfert ERP

```
GSC → API/EDI → ERP → Purchase Order + Inventory Management
```

---

## 🌐 Endpoints API

### 🔐 Authentification

```
POST   /api/auth/login                    - Connexion utilisateur
POST   /api/auth/register                 - Inscription utilisateur
POST   /api/auth/refresh                  - Rafraîchir token
POST   /api/auth/logout                   - Déconnexion
GET    /api/auth/profile                  - Profil utilisateur
```

### 🛫 Gestion des Vols

```
GET    /api/vols                          - Liste des vols (pagination)
GET    /api/vols/{id}                     - Détails d'un vol
POST   /api/vols                          - Créer un vol
PUT    /api/vols/{id}                     - Modifier un vol
DELETE /api/vols/{id}                     - Supprimer un vol
GET    /api/vols/search                   - Rechercher des vols
```

### 📋 Plans d'Hébergement

```
GET    /api/planshebergement              - Liste des plans
GET    /api/planshebergement/{id}         - Détails d'un plan
POST   /api/planshebergement              - Créer un plan
PUT    /api/planshebergement/{id}         - Modifier un plan
DELETE /api/planshebergement/{id}         - Supprimer un plan
POST   /api/planshebergement/{planId}/articles  - Ajouter article au plan
PUT    /api/planshebergement/articles/{id}      - Modifier article du plan
DELETE /api/planshebergement/articles/{id}      - Supprimer article du plan
POST   /api/planshebergement/{planId}/menus/{menuId}  - Associer menu
DELETE /api/planshebergement/{planId}/menus/{menuId}  - Dissocier menu
GET    /api/planshebergement/by-vol/{volId}     - Plan par vol
```

### 🍽️ Gestion des Menus

```
GET    /api/menus                         - Liste des menus
GET    /api/menus/{id}                    - Détails d'un menu
POST   /api/menus                         - Créer un menu
PUT    /api/menus/{id}                    - Modifier un menu
DELETE /api/menus/{id}                    - Supprimer un menu
POST   /api/menus/{menuId}/items          - Ajouter item au menu
PUT    /api/menus/items/{itemId}          - Modifier item du menu
DELETE /api/menus/items/{itemId}          - Supprimer item du menu
GET    /api/menus/search                  - Rechercher des menus
```

### 📦 Gestion des Articles

```
GET    /api/articles                      - Liste des articles
GET    /api/articles/{id}                 - Détails d'un article
POST   /api/articles                      - Créer un article
PUT    /api/articles/{id}                 - Modifier un article
DELETE /api/articles/{id}                 - Supprimer un article (soft)
GET    /api/articles/search               - Rechercher des articles
GET    /api/articles/by-type/{type}       - Articles par type
```

### 📝 Bons de Commande Prévisionnels (BCP)

```
GET    /api/bonscommanderevisionnels      - Liste des BCP
GET    /api/bonscommanderevisionnels/{id} - Détails d'un BCP
POST   /api/bonscommanderevisionnels      - Créer un BCP
PUT    /api/bonscommanderevisionnels/{id} - Modifier un BCP
DELETE /api/bonscommanderevisionnels/{id} - Supprimer un BCP
POST   /api/bonscommanderevisionnels/generate-from-vol/{volId}  - Générer BCP automatique
PUT    /api/bonscommanderevisionnels/{id}/status  - Changer statut BCP
GET    /api/bonscommanderevisionnels/search       - Rechercher des BCP
```

### 📋 Bons de Livraison (BL)

```
GET    /api/bonslivraison                 - Liste des BL
GET    /api/bonslivraison/{id}            - Détails d'un BL
POST   /api/bonslivraison                 - Créer un BL
PUT    /api/bonslivraison/{id}            - Modifier un BL
DELETE /api/bonslivraison/{id}            - Supprimer un BL
POST   /api/bonslivraison/{id}/validate   - Valider un BL (génère écarts)
PUT    /api/bonslivraison/{id}/status     - Changer statut BL
GET    /api/bonslivraison/search          - Rechercher des BL
```

### ⚠️ Gestion des Écarts

```
GET    /api/ecarts                        - Liste des écarts
GET    /api/ecarts/{id}                   - Détails d'un écart
POST   /api/ecarts                        - Créer un écart manuel
PUT    /api/ecarts/{id}                   - Modifier un écart
DELETE /api/ecarts/{id}                   - Supprimer un écart
POST   /api/ecarts/{id}/resolve           - Résoudre un écart
POST   /api/ecarts/{id}/accept            - Accepter un écart
POST   /api/ecarts/{id}/reject            - Rejeter un écart
GET    /api/ecarts/search                 - Rechercher des écarts
GET    /api/ecarts/statistics             - Statistiques des écarts
```

### 🏥 Boîtes Médicales

```
GET    /api/boitesmedicales               - Liste des boîtes médicales
GET    /api/boitesmedicales/{id}          - Détails d'une boîte
POST   /api/boitesmedicales               - Créer une boîte médicale
PUT    /api/boitesmedicales/{id}          - Modifier une boîte
DELETE /api/boitesmedicales/{id}          - Supprimer une boîte (soft)
POST   /api/boitesmedicales/{boiteId}/assign-to-vol/{volId}  - Assigner à un vol
DELETE /api/boitesmedicales/vol-assignments/{volBoiteId}     - Désassigner
GET    /api/boitesmedicales/available     - Boîtes disponibles
GET    /api/boitesmedicales/by-type/{type}  - Boîtes par type
GET    /api/boitesmedicales/expiring      - Boîtes bientôt expirées
GET    /api/boitesmedicales/search        - Rechercher des boîtes
```

### 📁 Dossiers de Vol

```
GET    /api/dossiersvol                   - Liste des dossiers
GET    /api/dossiersvol/{id}              - Détails d'un dossier
POST   /api/dossiersvol                   - Créer un dossier
PUT    /api/dossiersvol/{id}              - Modifier un dossier
DELETE /api/dossiersvol/{id}              - Supprimer un dossier
POST   /api/dossiersvol/generate-from-vol/{volId}  - Générer dossier automatique
POST   /api/dossiersvol/{id}/validate     - Valider un dossier
POST   /api/dossiersvol/{id}/archive      - Archiver un dossier
POST   /api/dossiersvol/{dossierId}/documents      - Ajouter document
DELETE /api/dossiersvol/documents/{documentId}     - Supprimer document
GET    /api/dossiersvol/by-vol/{volId}    - Dossier par vol
GET    /api/dossiersvol/search            - Rechercher des dossiers
```

### 📊 Rapports Budgétaires

```
GET    /api/rapportsbudgetaires           - Liste des rapports
GET    /api/rapportsbudgetaires/{id}      - Détails d'un rapport
POST   /api/rapportsbudgetaires/generate  - Générer un rapport
PUT    /api/rapportsbudgetaires/{id}      - Modifier un rapport
DELETE /api/rapportsbudgetaires/{id}      - Supprimer un rapport
POST   /api/rapportsbudgetaires/generate-comparison  - Rapport de comparaison
GET    /api/rapportsbudgetaires/performance-by-zone  - Performance par zone
GET    /api/rapportsbudgetaires/monthly-trends        - Tendances mensuelles
GET    /api/rapportsbudgetaires/search    - Rechercher des rapports
```

---

## 🗃️ Modèles de Données

### Vol

```json
{
  "id": 1,
  "flightNumber": "TU123",
  "flightDate": "2024-01-15",
  "departureTime": "08:30:00",
  "arrivalTime": "11:45:00",
  "aircraft": "A320",
  "origin": "Tunis",
  "destination": "Paris",
  "zone": "Europe",
  "estimatedPassengers": 150,
  "actualPassengers": 142,
  "duration": "03:15:00",
  "season": "Hiver"
}
```

### Plan d'Hébergement

```json
{
  "id": 1,
  "volId": 1,
  "name": "Plan TU123 - 15/01/2024",
  "description": "Plan standard Europe Hiver",
  "season": "Hiver",
  "aircraftType": "A320",
  "zone": "Europe",
  "flightDuration": "03:15:00",
  "isActive": true
}
```

### Article

```json
{
  "id": 1,
  "code": "REP001",
  "name": "Plateau repas Economy",
  "description": "Plateau repas standard classe économique",
  "type": "Repas",
  "unit": "Unité",
  "unitPrice": 12.5,
  "supplier": "NewRest",
  "isActive": true
}
```

### BCP (Bon de Commande Prévisionnel)

```json
{
  "id": 1,
  "numero": "BCP-TU123-20240115-001",
  "volId": 1,
  "dateCommande": "2024-01-10T10:00:00Z",
  "status": "Envoye",
  "fournisseur": "NewRest",
  "montantTotal": 1875.0,
  "lignes": [
    {
      "articleId": 1,
      "quantiteCommandee": 150,
      "prixUnitaire": 12.5,
      "montantLigne": 1875.0
    }
  ]
}
```

### BL (Bon de Livraison)

```json
{
  "id": 1,
  "numero": "BL-TU123-20240115-001",
  "volId": 1,
  "bonCommandePrevisionnelId": 1,
  "dateLivraison": "2024-01-15T06:00:00Z",
  "status": "Valide",
  "fournisseur": "NewRest",
  "montantTotal": 1800.0,
  "lignes": [
    {
      "articleId": 1,
      "quantiteLivree": 144,
      "prixUnitaire": 12.5,
      "montantLigne": 1800.0
    }
  ]
}
```

### Écart

```json
{
  "id": 1,
  "volId": 1,
  "articleId": 1,
  "bonCommandePrevisionnelId": 1,
  "bonLivraisonId": 1,
  "typeEcart": "QuantiteInferieure",
  "status": "EnAttente",
  "quantiteCommandee": 150,
  "quantiteLivree": 144,
  "ecartQuantite": -6,
  "ecartMontant": -75.0,
  "description": "Écart de quantité détecté"
}
```

---

## 🚀 Guide de Démarrage

### 1. Prérequis

- .NET 8.0 SDK
- SQL Server (LocalDB ou Server)
- Visual Studio 2022 ou VS Code

### 2. Installation

```bash
git clone [your-repo-url]
cd GSC.API
dotnet restore
```

### 3. Configuration Base de Données

```bash
# Mettre à jour la chaîne de connexion dans appsettings.json
# Appliquer les migrations
dotnet ef database update
```

### 4. Lancement

```bash
dotnet run
# API disponible sur : https://localhost:7001
# Swagger UI : https://localhost:7001/swagger
```

### 5. Authentification

```bash
# Utilisateur admin par défaut
Email: admin@gsc.com
Password: Admin123!
```

---

## 💡 Exemples d'Utilisation

### Workflow Complet - Exemple Pratique

#### 1. Créer un Vol

```http
POST /api/vols
{
  "flightNumber": "TU456",
  "flightDate": "2024-02-20",
  "departureTime": "14:30:00",
  "arrivalTime": "18:45:00",
  "aircraft": "A330",
  "origin": "Tunis",
  "destination": "Londres",
  "zone": "Europe",
  "estimatedPassengers": 280,
  "duration": "04:15:00",
  "season": "Hiver"
}
```

#### 2. Créer un Plan d'Hébergement

```http
POST /api/planshebergement
{
  "volId": 2,
  "name": "Plan TU456 - 20/02/2024",
  "season": "Hiver",
  "aircraftType": "A330",
  "zone": "Europe",
  "flightDuration": "04:15:00",
  "articles": [
    {
      "articleId": 1,
      "quantiteStandard": 1,
      "typePassager": "Economy"
    }
  ]
}
```

#### 3. Créer un Menu

```http
POST /api/menus
{
  "name": "Menu Europe Hiver",
  "typePassager": "Economy",
  "season": "Hiver",
  "zone": "Europe",
  "menuItems": [
    {
      "articleId": 1,
      "quantity": 1
    },
    {
      "articleId": 2,
      "quantity": 1
    }
  ]
}
```

#### 4. Générer un BCP Automatiquement

```http
POST /api/bonscommanderevisionnels/generate-from-vol/2
{
  "fournisseur": "NewRest"
}
```

#### 5. Créer un BL

```http
POST /api/bonslivraison
{
  "numero": "BL-TU456-20240220-001",
  "volId": 2,
  "bonCommandePrevisionnelId": 2,
  "dateLivraison": "2024-02-20T12:00:00Z",
  "fournisseur": "NewRest",
  "lignes": [
    {
      "articleId": 1,
      "quantiteLivree": 275,
      "prixUnitaire": 12.50
    }
  ]
}
```

#### 6. Valider le BL (Génère les Écarts Automatiquement)

```http
POST /api/bonslivraison/2/validate
```

#### 7. Traiter un Écart

```http
POST /api/ecarts/1/resolve
{
  "actionCorrective": "Écart accepté - 5 passagers de moins que prévu"
}
```

#### 8. Générer le Dossier de Vol

```http
POST /api/dossiersvol/generate-from-vol/2
```

#### 9. Générer un Rapport Budgétaire

```http
POST /api/rapportsbudgetaires/generate
{
  "titre": "Rapport Février 2024",
  "typeRapport": "Mensuel",
  "dateDebut": "2024-02-01",
  "dateFin": "2024-02-29"
}
```

---

## 📈 Statistiques et Monitoring

### Endpoints de Statistiques

- `GET /api/ecarts/statistics` - Statistiques des écarts
- `GET /api/rapportsbudgetaires/performance-by-zone` - Performance par zone
- `GET /api/rapportsbudgetaires/monthly-trends` - Tendances mensuelles

### Recherche Avancée

Tous les endpoints principaux supportent la recherche avec des paramètres de filtrage :

- Pagination : `?page=1&pageSize=10`
- Filtres par date : `?dateDebut=2024-01-01&dateFin=2024-01-31`
- Recherche textuelle : `?search=TU123`
- Filtres par statut : `?status=Valide`

---

## 🔒 Sécurité

- **Authentification JWT** : Tous les endpoints nécessitent une authentification
- **Autorisation basée sur les rôles** : Administrator, Manager, User, Viewer
- **Validation des données** : Validation côté serveur avec DataAnnotations
- **CORS configuré** : Pour les applications front-end

---

## 🛠️ Technologies Utilisées

- **Backend** : ASP.NET Core 8.0
- **ORM** : Entity Framework Core
- **Base de Données** : SQL Server
- **Authentification** : JWT + ASP.NET Identity
- **Documentation** : Swagger/OpenAPI
- **Architecture** : Clean Architecture avec Repository Pattern

---

## 📝 Notes Importantes

1. **Génération Automatique** : Les BCP peuvent être générés automatiquement à partir des plans d'hébergement et menus
2. **Rapprochement Automatique** : Les écarts sont générés automatiquement lors de la validation des BL
3. **Workflow Flexible** : Chaque étape peut être effectuée manuellement ou automatiquement
4. **Traçabilité Complète** : Toutes les actions sont tracées avec dates et utilisateurs
5. **Extensibilité** : L'architecture permet l'ajout facile de nouvelles fonctionnalités

---

## 🎯 Prochaines Étapes pour le Frontend

1. **Authentification** : Implémenter la connexion/inscription
2. **Dashboard** : Vue d'ensemble des vols et statistiques
3. **Gestion des Vols** : Interface CRUD pour les vols
4. **Plans d'Hébergement** : Interface de création et modification
5. **Workflow BCP/BL** : Interface de suivi du processus
6. **Gestion des Écarts** : Interface de traitement des écarts
7. **Rapports** : Interface de génération et visualisation des rapports

---

**🎉 L'API GSC est maintenant complète et prête pour l'intégration avec votre frontend !**

Tous les endpoints sont documentés, testables via Swagger, et suivent les meilleures pratiques REST. Le workflow complet de Tunisair est implémenté avec toutes les fonctionnalités demandées.
