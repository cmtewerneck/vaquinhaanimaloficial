import { DOCUMENT } from '@angular/common';
import { AfterViewInit, Component, Inject, OnInit } from '@angular/core';

@Component({
  selector: 'app-homepage',
  templateUrl: './homepage.component.html'
})
export class HomepageComponent implements OnInit, AfterViewInit {

  constructor(@Inject(DOCUMENT) private _document: any) { }

  ngOnInit() {
    var window = this._document.defaultView;
    window.ajustarPromoSlider();
    window.ajustarDonorsSlider();
  }

  ngAfterViewInit() {

  }
}
