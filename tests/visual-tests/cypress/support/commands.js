import { addMatchImageSnapshotCommand } from 'cypress-image-snapshot/command';
 
addMatchImageSnapshotCommand({
    failureThreshold: 0.03,
    failureThresholdType: 'percent',
    customDiffConfig: { threshold: 0.1 }
})