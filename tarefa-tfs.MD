Projectos a migrar:
Reservations
    $/Omnibees/Omnibees/0 release/API/OB.API.Reservations-release
SQL Reservations
$/Omnibees/Data/Reservations/Reservations_DB-pp

Gitflow Development:
Migrar Branchs TFVC <release\prod> para um branch main em GIT. A partir do main branch criar o develop branch.
https://docs.omnibees.dev/docs/agile/work-methodologies/branches-and-environments#gitflow-development-specific-subject-to-annalysis-per-project-needs

Pipelines:
Packages:
https://tfs.omnibees.com/tfs/IDTProjects/Omnibees/_apps/hub/ms.vss-ciworkflow.build-ci-hub?_a=edit-build-definition&id=171&view=Tab_Tasks  
Code:
https://tfs.omnibees.com/tfs/IDTProjects/Omnibees/_apps/hub/ms.vss-ciworkflow.build-ci-hub?_a=edit-build-definition&id=156

Deploys:
branch develop -> Ambiente de DEV  
branch release -> Ambiente de QA  
branch main -> Ambientes de PP\PROD\CERT

Packages:
Gera-se 2 packages (develop e main)
Em projectos que precisem destes packages a relação vai ser:
master -> develop
stable\release -> main

Novos packages gerados vão para o novo artefactos (Azure DevOps Artifacts Nugets)

Projectos Referencia:
https://tfs.omnibees.com/tfs/IDTProjects/Omnibees/_build?definitionId=756

Estimated Delivery: 16/05

Assim que tiver tudo testado, divulgamos data de migração para GIT, data essa em que serão bloqueados os branchs de TFVC.