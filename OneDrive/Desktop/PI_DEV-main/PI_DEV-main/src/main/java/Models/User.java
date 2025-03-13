package Models;

public class User {
    private int id;
    private String nom;
    private String prenom;
    private String image;
    private String email;
    private String MDP;

    // Constructor
    public User(int id, String nom, String prenom, String image, String email, String MDP) {
        this.id = id;
        this.nom = nom;
        this.prenom = prenom;
        this.image = image;
        this.email = email;
        this.MDP = MDP;
    }

    // Methods
    public void supprimeruser() {
        // Implementation for supprimeruser
    }

    public void ajouterUser() {
        // Implementation for ajouterUser
    }

    public void consulterprofil() {
        // Implementation for consulterprofil
    }

    public void modifierprofil() {
        // Implementation for modifierprofil
    }

    // Getters and Setters
    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public String getNom() { return nom; }
    public void setNom(String nom) { this.nom = nom; }

    public String getPrenom() { return prenom; }
    public void setPrenom(String prenom) { this.prenom = prenom; }

    public String getImage() { return image; }
    public void setImage(String image) { this.image = image; }

    public String getEmail() { return email; }
    public void setEmail(String email) { this.email = email; }

    public String getMDP() { return MDP; }
    public void setMDP(String MDP) { this.MDP = MDP; }

    // ToString method
    @Override
    public String toString() {
        return "User{" +
                "id=" + id +
                ", nom='" + nom + '\'' +
                ", prenom='" + prenom + '\'' +
                ", image='" + image + '\'' +
                ", email='" + email + '\'' +
                ", MDP='" + MDP + '\'' +
                '}';
    }
}
