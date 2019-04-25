import { Injectable } from "@angular/core";
import { FormBuilder, Validators, FormGroup } from "@angular/forms";
import { HttpClient, HttpHeaders } from "@angular/common/http";

@Injectable({
  providedIn: "root"
})
export class UserService {
  constructor(private fb: FormBuilder, private http: HttpClient) {}
  readonly BaseURI = "http://localhost:51638/api";

  //define form cotrolls
  formModel = this.fb.group({
    //default valui and validators
    UserName: ["", Validators.required],
    Email: ["", Validators.email],
    FullName: [""],
    //another group object for password
    Passwords: this.fb.group(
      {
        // if there more validators, they are passed by array
        Password: ["", [Validators.required, Validators.minLength(4)]],
        ConfirmPassword: ["", Validators.required]
      },
      { validator: this.comparePasswords }
    )
  });

  comparePasswords(fb: FormGroup) {
    let confirmPswrdCtrl = fb.get("ConfirmPassword");

    //here compare the passwords
    //passwordMismatch
    //confirmPswrdCtrl.errors={passwordMismatch:true}
    if (
      confirmPswrdCtrl.errors == null ||
      "passwordMismatch" in confirmPswrdCtrl.errors
    ) {
      if (fb.get("Password").value != confirmPswrdCtrl.value)
        confirmPswrdCtrl.setErrors({ passwordMismatch: true });
      else confirmPswrdCtrl.setErrors(null);
    }
  }

  register() {
    var body = {
      UserName: this.formModel.value.UserName,
      Email: this.formModel.value.Email,
      FullName: this.formModel.value.FullName,
      Password: this.formModel.value.Passwords.Password
    };
    return this.http.post(this.BaseURI + "/ApplicationUser/Register", body);
  }

  login(formData) {
    return this.http.post(this.BaseURI + "/ApplicationUser/Login", formData);
  }

  getUserProfile() {
    // console.log(localStorage.getItem("token"));
    // var tokenHeader = new HttpHeaders({
    //   Authorization: "Bearer " + localStorage.getItem("token")
    // });
    // return this.http.get(this.BaseURI + "/UserProfile", {
    //   headers: tokenHeader
    // });
    return this.http.get(this.BaseURI + "/UserProfile");
  }

  roleMatch(allowedRoles): boolean {
    var isMatch = false;
    var payLoad = JSON.parse(
      window.atob(localStorage.getItem("token").split(".")[1])
    );
    var userRole = payLoad.role;
    allowedRoles.forEach(element => {
      if (userRole == element) {
        isMatch = true;
        return false;
      }
    });
    return isMatch;
  }
}
