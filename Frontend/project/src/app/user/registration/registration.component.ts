import { Component, OnInit } from "@angular/core";
import { UserService } from "src/app/shared/user.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-registration",
  templateUrl: "./registration.component.html",
  styleUrls: ["./registration.component.css"]
})
export class RegistrationComponent implements OnInit {
  constructor(public service: UserService, private toastr: ToastrService) {}

  ngOnInit() {
    this.service.formModel.reset();
  }
  onSubmit() {
    this.service.register().subscribe(
      (res: any) => {
        if (res.succeeded) {
          this.service.formModel.reset();
          this.toastr.success("New User Created!", "Registration Sucessful.");
        } else {
          res.errors.forEach(element => {
            switch (element.code) {
              case "DuplicateUserName":
                //user name is already taken
                this.toastr.error(
                  "Username is already taken.",
                  "Registration failed."
                );
                break;
              default:
                //Regustration fail
                this.toastr.error(element.description, "Registration failed.");
                break;
            }
          });
        }
      },
      err => {}
    );
  }
}
