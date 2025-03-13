package tn.esprit.spring.kaddem.services;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import lombok.extern.slf4j.Slf4j;
import tn.esprit.spring.kaddem.entities.Contrat;
import tn.esprit.spring.kaddem.entities.Etudiant;
import tn.esprit.spring.kaddem.entities.Specialite;
import tn.esprit.spring.kaddem.repositories.ContratRepository;
import tn.esprit.spring.kaddem.repositories.EtudiantRepository;

import java.util.Date;
import java.util.List;
import java.util.Set;
import java.util.Calendar;

@Slf4j
@Service
public class ContratServiceImpl implements IContratService{
@Autowired
ContratRepository contratRepository;
@Autowired
	EtudiantRepository etudiantRepository;
	public List<Contrat> retrieveAllContrats(){
		return (List<Contrat>) contratRepository.findAll();
	}

	public Contrat updateContrat (Contrat  ce){
		return contratRepository.save(ce);
	}

	public  Contrat addContrat (Contrat ce){
		return contratRepository.save(ce);
	}

	public Contrat retrieveContrat (Integer  idContrat){
		return contratRepository.findById(idContrat).orElse(null);
	}

	public  void removeContrat(Integer idContrat){
		Contrat c=retrieveContrat(idContrat);
		contratRepository.delete(c);
	}



	public Contrat affectContratToEtudiant (Integer idContrat, String nomE, String prenomE){
		Etudiant e=etudiantRepository.findByNomEAndPrenomE(nomE, prenomE);
		Contrat ce=contratRepository.findByIdContrat(idContrat);
		Set<Contrat> contrats= e.getContrats();
		Integer nbContratssActifs=0;
		if (contrats.size()!=0) {
			for (Contrat contrat : contrats) {
				if (((contrat.getArchive())!=null)&& ((contrat.getArchive())!=false))  {
					nbContratssActifs++;
				}
			}
		}
		if (nbContratssActifs<=4){
		ce.setEtudiant(e);
		contratRepository.save(ce);}
		return ce;
	}
	public 	Integer nbContratsValides(Date startDate, Date endDate){
		return contratRepository.getnbContratsValides(startDate, endDate);
	}

	public void retrieveAndUpdateStatusContrat(){
		List<Contrat>contrats=contratRepository.findAll();
		List<Contrat>contrats15j=null;
		List<Contrat>contratsAarchiver=null;
		for (Contrat contrat : contrats) {
			Date dateSysteme = new Date();
			if (contrat.getArchive()==false) {
				long difference_In_Time = dateSysteme.getTime() - contrat.getDateFinContrat().getTime();
				long difference_In_Days = (difference_In_Time / (1000 * 60 * 60 * 24)) % 365;
				if (difference_In_Days==15){
					contrats15j.add(contrat);
					log.info(" Contrat : " + contrat);
				}
				if (difference_In_Days==0) {
					contratsAarchiver.add(contrat);
					contrat.setArchive(true);
					contratRepository.save(contrat);
				}
			}
		}
	}
	public float getChiffreAffaireEntreDeuxDates(Date startDate, Date endDate){
		float difference_In_Time = endDate.getTime() - startDate.getTime();
		float difference_In_Days = (difference_In_Time / (1000 * 60 * 60 * 24)) % 365;
		float difference_In_months =difference_In_Days/30;
        List<Contrat> contrats=contratRepository.findAll();
		float chiffreAffaireEntreDeuxDates=0;
		for (Contrat contrat : contrats) {
			if (contrat.getSpecialite()== Specialite.IA){
				chiffreAffaireEntreDeuxDates+=(difference_In_months*300);
			} else if (contrat.getSpecialite()== Specialite.CLOUD) {
				chiffreAffaireEntreDeuxDates+=(difference_In_months*400);
			}
			else if (contrat.getSpecialite()== Specialite.RESEAUX) {
				chiffreAffaireEntreDeuxDates+=(difference_In_months*350);
			}
			else //if (contrat.getSpecialite()== Specialite.SECURITE)
			 {
				 chiffreAffaireEntreDeuxDates+=(difference_In_months*450);
			}
		}
		return chiffreAffaireEntreDeuxDates;


	}

    @Override
    public Contrat renewContract(Integer idContrat, Integer newDuration) {
        log.info("Starting contract renewal process for contract ID: {}", idContrat);
        log.debug("Requested new duration: {} months", newDuration);

        // Get existing contract
        Contrat existingContract = contratRepository.findById(idContrat)
                .orElseThrow(() -> {
                    log.error("Contract not found with ID: {}", idContrat);
                    return new IllegalArgumentException("Contract not found with ID: " + idContrat);
                });
        
        log.debug("Found existing contract: specialite={}, montant={}, archive={}", 
            existingContract.getSpecialite(), 
            existingContract.getMontantContrat(),
            existingContract.getArchive());

        // Validation 1: Contract must not be archived
        if (Boolean.TRUE.equals(existingContract.getArchive())) {
            log.error("Attempted to renew archived contract: {}", idContrat);
            throw new IllegalStateException("Cannot renew an archived contract");
        }

        // Validation 2: Contract must be within 1 month of expiration
        Date now = new Date();
        long monthInMillis = 30L * 24 * 60 * 60 * 1000;
        long timeUntilExpiration = existingContract.getDateFinContrat().getTime() - now.getTime();
        long daysUntilExpiration = timeUntilExpiration / (24 * 60 * 60 * 1000);
        
        log.debug("Contract expiration check - Days until expiration: {}", daysUntilExpiration);
        
        if (timeUntilExpiration < 0 || timeUntilExpiration > monthInMillis) {
            log.error("Contract {} renewal rejected - Not within renewal window. Days until expiration: {}", 
                idContrat, daysUntilExpiration);
            throw new IllegalStateException("Contract can only be renewed within 1 month of expiration");
        }

        // Validation 3: Check student's active contracts
        Etudiant student = existingContract.getEtudiant();
        if (student == null) {
            log.error("Contract {} is not assigned to any student", idContrat);
            throw new IllegalStateException("Contract is not assigned to any student");
        }

        log.debug("Checking active contracts for student: id={}, nom={}, prenom={}", 
            student.getIdEtudiant(), student.getNomE(), student.getPrenomE());

        long activeContracts = student.getContrats().stream()
                .filter(c -> !Boolean.TRUE.equals(c.getArchive()))
                .count();
        
        log.debug("Student has {} active contracts", activeContracts);
        
        if (activeContracts > 3) {
            log.error("Student {} has too many active contracts: {}", student.getIdEtudiant(), activeContracts);
            throw new IllegalStateException("Student has too many active contracts");
        }

        // Calculate new contract amount with bonus/penalty based on history
        int baseAmount = calculateBaseAmount(existingContract.getSpecialite());
        float multiplier = calculateMultiplier(student);
        int newAmount = Math.round(baseAmount * multiplier);
        
        log.info("Calculated new contract amount: base={}, multiplier={}, final={}", 
            baseAmount, multiplier, newAmount);

        // Create renewed contract
        Contrat renewedContract = new Contrat();
        renewedContract.setDateDebutContrat(existingContract.getDateFinContrat());
        
        Calendar calendar = Calendar.getInstance();
        calendar.setTime(existingContract.getDateFinContrat());
        calendar.add(Calendar.MONTH, newDuration);
        Date newEndDate = calendar.getTime();
        renewedContract.setDateFinContrat(newEndDate);
        
        renewedContract.setSpecialite(existingContract.getSpecialite());
        renewedContract.setArchive(false);
        renewedContract.setMontantContrat(newAmount);
        renewedContract.setEtudiant(student);

        log.debug("Created new contract: startDate={}, endDate={}, specialite={}, montant={}", 
            renewedContract.getDateDebutContrat(),
            renewedContract.getDateFinContrat(),
            renewedContract.getSpecialite(),
            renewedContract.getMontantContrat());

        // Archive old contract
        existingContract.setArchive(true);
        contratRepository.save(existingContract);
        log.info("Archived original contract: {}", idContrat);

        // Save and return new contract
        Contrat savedContract = contratRepository.save(renewedContract);
        log.info("Successfully created renewed contract with ID: {}", savedContract.getIdContrat());
        
        return savedContract;
    }

    private int calculateBaseAmount(Specialite specialite) {
        int amount = switch (specialite) {
            case IA -> 1200;
            case CLOUD -> 1000;
            case SECURITE -> 1300;
            case RESEAUX -> 900;
            default -> 800;
        };
        log.debug("Calculated base amount for specialite {}: {}", specialite, amount);
        return amount;
    }

    private float calculateMultiplier(Etudiant student) {
        // Count successfully completed contracts
        long completedContracts = student.getContrats().stream()
                .filter(c -> Boolean.TRUE.equals(c.getArchive()))
                .count();
        
        log.debug("Student {} has {} completed contracts", student.getIdEtudiant(), completedContracts);

        // Apply loyalty bonus
        float multiplier;
        if (completedContracts > 2) {
            multiplier = 1.2f;
            log.info("Applied 20% loyalty bonus for student {} with {} completed contracts", 
                student.getIdEtudiant(), completedContracts);
        } else if (completedContracts > 0) {
            multiplier = 1.1f;
            log.info("Applied 10% loyalty bonus for student {} with {} completed contracts", 
                student.getIdEtudiant(), completedContracts);
        } else {
            multiplier = 1.0f;
            log.debug("No loyalty bonus applied for student {} (new student)", student.getIdEtudiant());
        }
        
        return multiplier;
    }
}
