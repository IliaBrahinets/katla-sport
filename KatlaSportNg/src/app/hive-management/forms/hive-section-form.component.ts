import { Component, OnInit } from '@angular/core';
import { HiveSectionService } from 'app/hive-management/services/hive-section.service';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from 'environments/environment';
import { HiveSection } from 'app/hive-management/models/hive-section';
import { HiveService } from 'app/hive-management/services/hive.service';
import { HiveListItem } from 'app/hive-management/models/hive-list-item';
import { toInteger } from '@ng-bootstrap/ng-bootstrap/util/util';

@Component({
  selector: 'app-hive-section-form',
  templateUrl: './hive-section-form.component.html',
  styleUrls: ['./hive-section-form.component.css']
})
export class HiveSectionFormComponent implements OnInit {

  hiveSection = new HiveSection(0,'','',0,false,'');
  hives: HiveListItem[];

  storeHiveId:number;
  existed = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private hiveService: HiveService,
    private hiveSectionService: HiveSectionService
  ) { }

  ngOnInit() {
    this.hiveService.getHives().subscribe(hArr => this.hives = hArr);
    this.route.params.subscribe(p => {
      this.storeHiveId = toInteger(p['storeHiveId']);
      if(p['id'] === undefined) {
        this.hiveSection.storeHiveId = this.storeHiveId;
      } else {
        this.hiveSectionService.getHiveSection(p['id']).subscribe(hs => {
          this.hiveSection = hs;
          if(isNaN(this.storeHiveId)){
            this.storeHiveId = hs.storeHiveId;
          }
        });
        this.existed = true;
      }
    });
  }

  navigateToHivesSections() {
    this.router.navigate([`/hive/${this.storeHiveId}/sections`]);
  }

  onCancel() {
    this.navigateToHivesSections();
  }

  onSubmit(){
    if(this.existed){
      this.hiveSectionService.updateHiveSection(this.hiveSection)
                             .subscribe(e => this.navigateToHivesSections());
    } else {
      this.hiveSectionService.addHiveSection(this.hiveSection)
                             .subscribe(e => this.navigateToHivesSections());
    }
  }

  onDelete(){
    this.hiveSectionService.setHiveSectionStatus(this.hiveSection.id, true)
                           .subscribe(e => this.hiveSection.isDeleted = true);
  }

  onRestore(){
    this.hiveSectionService.setHiveSectionStatus(this.hiveSection.id, false)
                           .subscribe(e => this.hiveSection.isDeleted = false);
  }

  onPurge(){
    this.hiveSectionService.deleteHiveSection(this.hiveSection.id)
                           .subscribe(e => this.navigateToHivesSections());
  }
}
