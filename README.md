# 🚗 Tunis Motors - Plateforme E-commerce

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoft-sql-server)
![Tests](https://img.shields.io/badge/Tests-84%20Passed-success)
![Coverage](https://img.shields.io/badge/API%20Tests-69%2F69-brightgreen)
![E2E](https://img.shields.io/badge/E2E%20Tests-15%2F15-brightgreen)
![License](https://img.shields.io/badge/License-Academic-blue)

**Plateforme e-commerce moderne pour la vente et location de véhicules en Tunisie**

[Fonctionnalités](#-fonctionnalités) • [Installation](#-installation) • [Architecture](#-architecture) • [Documentation](#-documentation)

</div>

---

## 📋 Table des Matières

- [À Propos](#-à-propos)
- [Fonctionnalités](#-fonctionnalités)
- [Technologies](#-technologies)
- [Architecture](#-architecture)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Utilisation](#-utilisation)
- [Structure du Projet](#-structure-du-projet)
- [API Documentation](#-api-documentation)
- [Captures d'Écran](#-captures-décran)
- [Auteurs](#-auteurs)

---

## 🎯 À Propos

**Tunis Motors** est une plateforme e-commerce complète développée pour révolutionner le marché automobile tunisien. Cette application web moderne permet aux utilisateurs d'acheter et de louer des véhicules en ligne avec une expérience utilisateur fluide et sécurisée.

### Caractéristiques Principales

- ✅ **Architecture Moderne** : Blazor WebAssembly (Frontend) + ASP.NET Core Web API (Backend)
- ✅ **Sécurité Renforcée** : Authentification JWT + Autorisation basée sur les rôles
- ✅ **Paiement Sécurisé** : Intégration Stripe pour les transactions
- ✅ **Système de Location** : Gestion complète de la location de véhicules avec calendrier
- ✅ **Interface Responsive** : Design moderne et adaptatif pour tous les appareils
- ✅ **Design Patterns** : Repository, Service, DTO, Dependency Injection

---

## ✨ Fonctionnalités

### 🔐 Authentification & Autorisation

- Inscription avec sélection de rôle
- Connexion sécurisée avec JWT
- Confirmation par email
- Récupération de mot de passe
- **4 Rôles** : Admin, Manager, Seller, Buyer
- Gestion des permissions par rôle

### 🚗 Gestion des Annonces

- **CRUD Complet** : Création, modification, suppression d'annonces
- **Recherche Avancée** : 11 filtres (catégorie, marque, prix, année, kilométrage, carburant, transmission, couleur, etc.)
- **Tri Multiple** : Par date, prix, popularité, kilométrage
- **Upload Multiple** : Jusqu'à 10 images par annonce
- **Workflow d'Approbation** : Validation par Admin/Manager avant publication
- **Gestion des Stocks** : Séparation stock vente / stock location

### 🛒 Système d'Achat

- Parcourir le catalogue de véhicules
- Ajouter au panier
- Processus de commande complet
- **Paiement Stripe** : Intégration sécurisée
- Suivi des commandes en temps réel
- Historique des commandes
- Factures imprimables

### 🏠 Système de Location

- Demande de location de véhicules
- Calendrier de disponibilité
- Approbation par le vendeur
- Gestion des périodes de location
- Calcul automatique des coûts
- Extension de location
- Retour de véhicule

### ⚖️ Comparaison de Véhicules

- Comparaison côte à côte (jusqu'à 4 véhicules)
- Tableau comparatif détaillé
- Spécifications techniques
- Prix et disponibilité

### ⭐ Liste de Souhaits

- Ajouter/Retirer des favoris
- Vue d'ensemble des favoris
- Accès rapide depuis les pages produits

### 💬 Messagerie

- Communication vendeur-acheteur
- Boîte de réception
- Messages envoyés
- Notifications en temps réel

### 📧 Notifications Email

- Confirmation d'inscription
- Confirmation de commande
- Mise à jour du statut des commandes
- Notifications de location
- Notifications d'approbation
- Emails de vente aux vendeurs

### 👨‍💼 Administration

- **Tableau de Bord** : Statistiques complètes
- **Gestion des Utilisateurs** : Création, modification, attribution de rôles
- **Validation des Annonces** : Approbation/Rejet avec notes
- **Gestion des Catégories** : CRUD complet
- **Gestion des Commandes** : Suivi et mise à jour des statuts
- **Analytics** : Statistiques de vente et performance

---

## 🛠️ Technologies

### Backend

| Technologie | Version | Description |
|------------|---------|-------------|
| **ASP.NET Core** | 8.0 | Framework web principal |
| **Entity Framework Core** | 9.0.9 | ORM pour accès base de données |
| **ASP.NET Identity** | 2.3.1 | Gestion authentification |
| **JWT Bearer** | 8.0.11 | Authentification par tokens |
| **Stripe.net** | 43.12.0 | Intégration paiements |
| **MailKit** | Latest | Service email SMTP |
| **SQL Server** | LocalDB | Base de données |

### Frontend

| Technologie | Version | Description |
|------------|---------|-------------|
| **Blazor WebAssembly** | 8.0 | Framework frontend |
| **Bootstrap** | 5.3.0 | Framework CSS |
| **Font Awesome** | Latest | Bibliothèque d'icônes |
| **Fluxor** | 6.0.0 | State management |
| **Blazored.LocalStorage** | 4.4.0 | Stockage local |

### Outils de Développement

- **Visual Studio 2022** ou **VS Code**
- **.NET 8 SDK**
- **SQL Server Management Studio** (optionnel)
- **Git** pour le contrôle de version

---

## 🏗️ Architecture

### Architecture en Trois Couches

```
┌─────────────────────────────────────┐
│   Couche Présentation (Frontend)    │
│   Blazor WebAssembly                │
│   - Pages Razor (40+)               │
│   - Services Client (18)            │
│   - Composants Réutilisables        │
└──────────────┬──────────────────────┘
               │ HTTP/HTTPS + JWT
┌──────────────▼──────────────────────┐
│   Couche Métier (Backend)           │
│   ASP.NET Core Web API              │
│   - Controllers (14)                │
│   - Services Métier (17)            │
│   - Repositories (7)                │
└──────────────┬──────────────────────┘
               │ Entity Framework
┌──────────────▼──────────────────────┐
│   Couche Données                    │
│   SQL Server (LocalDB)              │
│   - 9 Modèles de Domaine            │
│   - Migrations EF Core              │
└─────────────────────────────────────┘
```

### Design Patterns Implémentés

- **Repository Pattern** : Abstraction de l'accès aux données
- **Service Pattern** : Logique métier centralisée
- **DTO Pattern** : Transfert de données optimisé
- **Dependency Injection** : Couplage faible et testabilité

---

## 📦 Installation

### Prérequis

Assurez-vous d'avoir installé :

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Étapes d'Installation

1. **Cloner le dépôt**
   ```bash
   git clone https://github.com/ahmedKhlif/Tunis-Motors-Blazor.git
   cd Tunis-Motors-Blazor
   ```

2. **Configurer la Base de Données**
   - SQL Server LocalDB est utilisé par défaut
   - La base de données sera créée automatiquement au premier lancement

3. **Configurer le Backend**
   ```bash
   cd webappAPI/webappAPI
   ```
   - Modifier `appsettings.json` (voir section Configuration)
   - Exécuter les migrations :
     ```bash
     dotnet ef database update
     ```
   - Lancer l'API :
     ```bash
     dotnet run
     ```
   - L'API sera disponible sur : `http://localhost:5000`
   - Swagger UI : `http://localhost:5000/swagger`

4. **Configurer le Frontend**
   ```bash
   cd BlazorApp/BlazorApp
   ```
   - Modifier `wwwroot/appsettings.json` si nécessaire
   - Lancer l'application :
     ```bash
     dotnet run
     ```
   - L'application sera disponible sur : `http://localhost:5271`

---

## ⚙️ Configuration

### Backend Configuration (`webappAPI/webappAPI/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=(localdb)\\MSSQLLocalDB;database=TunisiaMotorsAPI;Integrated Security=true;"
  },
  "Jwt": {
    "SecretKey": "VOTRE_CLE_SECRETE_TRES_LONGUE_ET_SECURISEE",
    "Issuer": "TunisiaMotorsAPI",
    "Audience": "TunisiaMotorsClients",
    "ExpirationMinutes": 60
  },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_..."
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "votre-email@gmail.com",
    "SenderPassword": "votre-mot-de-passe-app"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5271"]
  }
}
```

### Frontend Configuration (`BlazorApp/BlazorApp/wwwroot/appsettings.json`)

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

### Génération d'une Clé JWT Secrète

```bash
# PowerShell
[Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes([System.Guid]::NewGuid().ToString() + [System.Guid]::NewGuid().ToString()))
```

---

## 🚀 Utilisation

### Comptes par Défaut

Après la première migration, créez un compte via l'interface d'inscription, puis assignez manuellement le rôle Admin via la base de données :

```sql
-- Trouver l'ID utilisateur
SELECT Id, Email FROM AspNetUsers WHERE Email = 'votre-email@example.com';

-- Assigner le rôle Admin
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.Email = 'votre-email@example.com' AND r.Name = 'Admin';
```

### Rôles Disponibles

- **Admin** : Accès complet à toutes les fonctionnalités
- **Manager** : Validation des annonces, gestion des catégories
- **Seller** : Création et gestion d'annonces, gestion des locations
- **Buyer** : Achat, location, comparaison, favoris

---

## 📁 Structure du Projet

```
Projet.net/
├── 📂 webappAPI/                    # Backend API
│   └── webappAPI/
│       ├── 📂 Controllers/          # 14 contrôleurs API
│       │   ├── AuthController.cs
│       │   ├── CarListingsController.cs
│       │   ├── OrdersController.cs
│       │   ├── CarRentalsController.cs
│       │   └── ...
│       ├── 📂 Services/             # 17 services métier
│       │   ├── IAuthService.cs
│       │   ├── AuthService.cs
│       │   ├── ICarListingService.cs
│       │   └── ...
│       ├── 📂 Repositories/         # 7 repositories
│       │   ├── ICarListingRepository.cs
│       │   ├── CarListingRepository.cs
│       │   └── ...
│       ├── 📂 Models/               # 9 modèles de domaine
│       │   ├── CarListing.cs
│       │   ├── Order.cs
│       │   └── ...
│       ├── 📂 DTOs/                 # 50+ DTOs
│       ├── 📂 Data/                 # AppDbContext + Migrations
│       └── appsettings.json
│
├── 📂 BlazorApp/                    # Frontend Blazor WASM
│   └── BlazorApp/
│       ├── 📂 Pages/                # 40+ pages Razor
│       │   ├── Account/
│       │   ├── Admin/
│       │   ├── Cart/
│       │   ├── Product/
│       │   ├── Rental/
│       │   └── Order/
│       ├── 📂 Services/             # 18 services client
│       │   ├── ApiClient.cs
│       │   ├── AuthService.cs
│       │   └── ...
│       ├── 📂 Components/           # Composants réutilisables
│       │   ├── NotificationDialog.razor
│       │   └── ...
│       ├── 📂 Layout/               # Layouts
│       │   ├── MainLayout.razor
│       │   └── PrintLayout.razor
│       └── wwwroot/
│
├── 📄 README.md                     # Ce fichier
├── 📄 rapport_final.tex             # Rapport LaTeX du projet
└── 📂 diagrammes/                   # Diagrammes du projet (PNG)
```

---

## 📚 API Documentation

### Endpoints Principaux

#### 🔐 Authentification
- `POST /api/auth/register` - Inscription
- `POST /api/auth/login` - Connexion
- `POST /api/auth/confirm-email` - Confirmation email
- `POST /api/auth/forgot-password` - Récupération mot de passe
- `POST /api/auth/reset-password` - Réinitialisation mot de passe

#### 🚗 Annonces de Véhicules
- `GET /api/carlistings` - Liste avec filtres
- `GET /api/carlistings/{id}` - Détails d'une annonce
- `POST /api/carlistings` - Créer une annonce
- `PUT /api/carlistings/{id}` - Modifier une annonce
- `DELETE /api/carlistings/{id}` - Supprimer une annonce
- `POST /api/carlistings/{id}/approve` - Approuver une annonce
- `POST /api/carlistings/{id}/reject` - Rejeter une annonce

#### 🛒 Commandes
- `GET /api/orders` - Liste des commandes utilisateur
- `GET /api/orders/{id}` - Détails d'une commande
- `POST /api/orders` - Créer une commande
- `PUT /api/orders/{id}/status` - Mettre à jour le statut
- `POST /api/orders/{id}/cancel` - Annuler une commande

#### 🏠 Locations
- `GET /api/carrentals` - Liste des locations
- `POST /api/carrentals/request` - Demander une location
- `POST /api/carrentals/{id}/approve` - Approuver une location
- `POST /api/carrentals/{id}/activate` - Activer une location
- `POST /api/carrentals/{id}/return` - Retourner un véhicule

#### 💳 Paiement
- `POST /api/orders/{id}/payment-intent` - Créer un intent de paiement
- `POST /api/orders/process-payment` - Traiter le paiement

### Documentation Swagger

Une fois l'API lancée, accédez à la documentation Swagger interactive :
```
http://localhost:5000/swagger
```

---

## 📸 Captures d'Écran

Les captures d'écran de l'application sont disponibles dans le rapport LaTeX (`rapport_final.tex`).

### Pages Principales

- 🏠 **Page d'Accueil** : Vue d'ensemble avec véhicules en vedette
- 🛍️ **Catalogue** : Liste des véhicules avec filtres avancés
- 🚗 **Détails Produit** : Carrousel d'images, spécifications, actions
- 🛒 **Panier** : Gestion des articles
- 💳 **Checkout** : Processus de paiement avec Stripe
- 👨‍💼 **Tableau de Bord Admin** : Statistiques et gestion
- ✅ **Approbations** : Interface de validation des annonces
- 📅 **Gestion Locations** : Calendrier et gestion des locations
- ⚖️ **Comparaison** : Comparaison côte à côte
- 🧾 **Facture** : Facture imprimable

---

## 👥 Auteurs

**Ahmed Khlif** & **Wissem Hajbi**

- **Institution** : L'Institut International de Technologie (IIT)
- **Classe** : GL2ID
- **Année Universitaire** : 2025/2026
- **Matière** : Programmation .NET
- **Enseignant** : M. Fahmi KALLEL

---

## 📄 Rapport

Le rapport complet du projet (format LaTeX) est disponible dans `rapport_final.tex`.

Le rapport inclut :
- Introduction et contexte
- Analyse fonctionnelle
- Diagrammes (Cas d'utilisation, Séquence, ER, Classes, etc.)
- Architecture technique
- Design patterns et leurs avantages
- Captures d'écran
- Tests et validation
- Conclusion

---

## 🔗 Liens Utiles

- **Dépôt GitHub** : [https://github.com/ahmedKhlif/Tunis-Motors-Blazor](https://github.com/ahmedKhlif/Tunis-Motors-Blazor)
- **Documentation .NET** : [https://learn.microsoft.com/dotnet/](https://learn.microsoft.com/dotnet/)
- **Documentation Blazor** : [https://learn.microsoft.com/aspnet/core/blazor/](https://learn.microsoft.com/aspnet/core/blazor/)
- **Documentation Stripe** : [https://stripe.com/docs](https://stripe.com/docs)

---

## 📜 Licence

Ce projet est développé dans le cadre d'un projet académique à l'Institut International de Technologie (IIT).

---

## ✅ Status du Projet

**🟢 Projet Complet et Fonctionnel**

- ✅ Backend API : 100% fonctionnel
- ✅ Frontend Blazor : 100% fonctionnel
- ✅ Authentification : Implémentée
- ✅ Paiement Stripe : Intégré
- ✅ Système de Location : Complet
- ✅ Administration : Complète
- ✅ Tests : Validés

---

<div align="center">

**Fait avec ❤️ par Ahmed Khlif & Wissem Hajbi**

⭐ Si ce projet vous a été utile, n'hésitez pas à mettre une étoile !

</div>
