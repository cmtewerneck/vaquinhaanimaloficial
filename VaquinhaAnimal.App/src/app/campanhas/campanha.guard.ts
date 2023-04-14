import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, ActivatedRoute } from '@angular/router';
import { LocalStorageUtils } from '../_utils/localStorage';

@Injectable()
export class CampanhaGuard implements CanActivate {

    localStorageUtils = new LocalStorageUtils();

    constructor(private router: Router, private route: ActivatedRoute){}

    canActivate() {
        if(!this.localStorageUtils.obterTokenUsuarioSession()){
            this.router.navigate(['/auth/login/'], { queryParams: { returnUrl: '/campanhas/criar' }});
        }

        return true;  
    }
}