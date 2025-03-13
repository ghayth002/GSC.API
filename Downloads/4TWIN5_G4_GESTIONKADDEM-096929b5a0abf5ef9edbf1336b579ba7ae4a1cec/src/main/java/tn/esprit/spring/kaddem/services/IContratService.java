package tn.esprit.spring.kaddem.services;

import tn.esprit.spring.kaddem.entities.Contrat;
import java.util.Date;
import java.util.List;

public interface IContratService {
    public List<Contrat> retrieveAllContrats();

    public Contrat updateContrat (Contrat  ce);

    public  Contrat addContrat (Contrat ce);

    public Contrat retrieveContrat (Integer  idContrat);

    public  void removeContrat(Integer idContrat);

    public Contrat affectContratToEtudiant (Integer idContrat, String nomE, String prenomE);

        public 	Integer nbContratsValides(Date startDate, Date endDate);


    public float getChiffreAffaireEntreDeuxDates(Date startDate, Date endDate);

    public void retrieveAndUpdateStatusContrat();

    /**
     * Renews a contract with validation rules:
     * 1. Contract must not be archived
     * 2. Contract must be within 1 month of expiration
     * 3. Student must have good academic standing (no more than 3 active contracts)
     * 4. New contract amount is calculated based on speciality and previous contract history
     * 
     * @param idContrat The ID of the contract to renew
     * @param newDuration Duration in months for the renewed contract
     * @return The renewed contract with updated terms
     * @throws IllegalStateException if renewal conditions are not met
     */
    public Contrat renewContract(Integer idContrat, Integer newDuration);
}

