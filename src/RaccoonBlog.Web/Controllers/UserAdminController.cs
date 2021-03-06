using System.Linq;
using System.Web.Mvc;
using RaccoonBlog.Web.Infrastructure.AutoMapper;
using RaccoonBlog.Web.Infrastructure.AutoMapper.Profiles.Resolvers;
using RaccoonBlog.Web.Models;
using RaccoonBlog.Web.ViewModels;

namespace RaccoonBlog.Web.Controllers
{
    public class UserAdminController : AdminController
    {
    	public ActionResult List()
        {
            var users = Session.Query<User>()
                .OrderBy(u => u.FullName)
                .ToList();

            var vm = users.MapTo<UserSummeryViewModel>();
            return View(vm);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View("Edit", new UserInput());
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var user = Session.Load<User>(id);
            if (user == null)
                return HttpNotFound("User does not exist.");
        	return View(user.MapTo<UserInput>());
        }

        [HttpPost]
        public ActionResult Update(UserInput input)
        {
        	if (!ModelState.IsValid)
        		return View("Edit", input);

        	var user = Session.Load<User>(input.Id) ?? new User();
        	input.MapPropertiesToInstance(user);
        	Session.Store(user);
        	return RedirectToAction("List");
        }

        [HttpGet]
        public ActionResult ChangePass(int id)
        {
            var user = Session.Load<User>(id);
            if (user == null)
                return HttpNotFound("User does not exist.");

            return View(new ChangePassViewModel
                            {
                                FullName = user.FullName,
                                Input = new UserPasswordInput {Id = RavenIdResolver.Resolve(user.Id)}
                            });
        }

		[HttpPost]
		public ActionResult ChangePass(UserPasswordInput input)
		{
			var user = Session.Load<User>(input.Id);
			if (user == null)
				return HttpNotFound("User does not exist.");

		    if (user.ValidatePassword(input.OldPass) == false)
		    {
		        ModelState.AddModelError("OldPass", "Old password did not match existing password");
		    }

			if (ModelState.IsValid == false)
				return View(new ChangePassViewModel { FullName = user.FullName, Input = input });

            user.SetPassword(input.NewPass);
		    return RedirectToAction("List");
		}

        [HttpPost]
		public ActionResult SetActivation(int id, bool isActive)
		{
			var user = Session.Load<User>(id);
			if (user == null)
				return HttpNotFound("User does not exist.");

			user.Enabled = isActive;

			return RedirectToAction("List");
		}
    }
}
