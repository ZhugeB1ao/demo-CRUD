# 🧪 Complete Checkout & Payment History Testing Guide

## Step-by-Step Testing

### ✅ Test 1: Create Your First Order

#### Step 1: Start the Application
```bash
cd WebApplication1
dotnet run
```

Visit: `https://localhost:5001`

#### Step 2: Browse & Add Items to Cart
- Click "Shop" in navbar
- Click "Add to Cart" on any product (e.g., iPhone 15)
- Add quantity if needed
- Repeat with more products if desired
- Verify cart icon shows item count

#### Step 3: Go to Cart
- Click "Cart" in navbar (or cart icon)
- Verify items display with prices
- Check order summary calculates correctly

#### Step 4: Click Checkout
- Click "Checkout" button
- **If NOT logged in:**
  - Should redirect to `/User/Login`
  - Your beautiful login page appears
  - Login with your credentials
  - After login, manually go back to cart or shop
- **If already logged in:**
  - Order created immediately
  - Cart cleared
  - Success message shows
  - Redirected to Shop

#### Step 5: Verify Order Saved
- Go to cart again
- Cart should be empty
- Order was stored in database ✅

---

### ✅ Test 2: View Payment History

#### Step 1: Stay Logged In (or login if needed)
- Verify you can see your username in navbar

#### Step 2: Click Payment History Icon
- Look at top-right corner of navbar
- Find icon next to "Cart" (looks like: ⏱ or 📋 or 🕒)
- Click it

#### Step 3: Wait for Modal to Load
- Modal dialog opens
- Should show: "Payment History" title
- Loading spinner disappears
- **Expected:** Table with your order(s)

**Table should show:**
```
Order Date  | Items | Total    | Status
Nov 28, 2025| 2     | $899.99  | Pending
```

#### Step 4: Click on Order
- Click anywhere on the order row
- Row should expand
- Shows items in that order:
  ```
  iPhone 15 Pro × 1  $999.00
  Case × 2            $45.00
  ```

#### Step 5: Click Again to Collapse
- Click same order row again
- Details should hide
- Order row shows summary again

---

### ✅ Test 3: Create Multiple Orders

#### Step 1: Add Different Items
- Go to Shop
- Add **different** products to cart
- (Use products not in previous order)

#### Step 2: Checkout Again
- Go to Cart
- Click Checkout
- Order created with new items

#### Step 3: Check Payment History
- Click history icon
- **Should see TWO orders now:**
  ```
  Nov 28, 2025 | 2 | $599.99  | Pending
  Nov 28, 2025 | 1 | $899.99  | Pending
  ```
- Both listed, newest first
- Each expandable separately

---

### ✅ Test 4: User Isolation

#### Step 1: Create Order as User A
- Logged in as: user1@example.com
- Create an order
- View payment history
- Note the orders shown

#### Step 2: Logout
- Find logout button (usually top-right)
- Click to logout

#### Step 3: Login as Different User (User B)
- Login with: user2@example.com
- (Or register new account if needed)

#### Step 4: View Payment History
- Click payment history icon
- **Should see DIFFERENT orders** (User B's only)
- **Should NOT see User A's orders!** ✅

#### Step 5: Verify Isolation
- User A's orders: Only visible to User A
- User B's orders: Only visible to User B
- No data leakage between users

---

## Debugging Checklist

### If "Loading your payment history..." never goes away:

**Check 1: Browser Console**
```
Press F12
Go to Console tab
Look for red error messages
Report any errors you see
```

**Check 2: Network Tab**
```
Press F12
Go to Network tab
Click payment history icon
Look for requests to /Cart/PaymentHistory
Check if it returns 200 (success) or 404/500 (error)
```

**Check 3: Did you place an order?**
```
- Make sure you actually clicked checkout
- Cart should have been cleared
- Check browser's success message appeared
- If no success message, order wasn't created
```

**Check 4: Are you logged in?**
```
- History icon only shows for logged-in users
- Check navbar for username or user info
- If not logged in, icon won't appear
```

---

## What Should Happen

### Timeline of Events

```
1. Customer: Clicks Checkout
   ↓
2. System: Check if authenticated
   ├─ NO  → Redirect to /User/Login
   └─ YES → Continue
   ↓
3. System: Get cart items
   ↓
4. System: Calculate total (server-side)
   ↓
5. System: Create Order object
   ↓
6. System: Save to database
   ↓
7. System: Clear cart from session
   ↓
8. Browser: Shows success message
   ↓
9. Browser: Redirects to shop
   ↓
10. Database: Order is now permanently saved
    ↓
11. Customer: Later clicks history icon
    ↓
12. Browser: Sends AJAX request to /Cart/PaymentHistory
    ↓
13. Server: Queries: SELECT * FROM Orders WHERE UserId = customer
    ↓
14. Server: Returns HTML table with all customer's orders
    ↓
15. Browser: Shows table in modal
    ↓
16. Customer: Can expand/collapse each order
```

---

## Expected Results

### ✅ Successful Checkout
```
✓ Cart items add without error
✓ Checkout button is clickable
✓ If not logged in: Redirected to login
✓ If logged in: Order created immediately
✓ Success message appears briefly
✓ Cart becomes empty
✓ Redirected to shop
```

### ✅ Successful Payment History
```
✓ Payment history icon visible (when logged in)
✓ Click opens modal dialog
✓ Modal loads orders (not stuck on "Loading...")
✓ Orders table appears with your orders
✓ Can see: Date, Item count, Total, Status
✓ Click order: Details expand showing items
✓ Click again: Details collapse
✓ Only YOUR orders shown (not other users')
```

---

## Database Verification (SQL Server)

### View All Orders
```sql
SELECT * FROM Orders;
```

Should show records like:
```
Id | UserId              | Status  | CreatedAt           | Total
1  | user1@example.com   | Pending | 2025-11-28 10:30:00 | 899.99
2  | user1@example.com   | Pending | 2025-11-28 11:15:00 | 599.99
3  | user2@example.com   | Pending | 2025-11-28 12:00:00 | 299.99
```

### View Order Items
```sql
SELECT * FROM OrderProducts;
```

Should show records like:
```
OrderId | ProductId | Quantity
1       | 5         | 1
1       | 8         | 2
2       | 3         | 1
3       | 7         | 3
```

---

## Common Issues & Solutions

### Issue: "Loading your payment history..." never completes

**Cause:** JavaScript fetch is not receiving response

**Solution:**
1. Check browser console (F12) for errors
2. Check Network tab to see if `/Cart/PaymentHistory` request exists
3. Make sure you're logged in
4. Restart the application: `Ctrl+C` then `dotnet run`

---

### Issue: Payment history icon doesn't appear

**Cause:** Icon only shows when logged in

**Solution:**
1. Make sure you're logged in (check navbar)
2. Check if you see your username/email
3. If not logged in, login first
4. Icon should appear after login

---

### Issue: No orders show even after checkout

**Cause:** Orders not created or not saved

**Solution:**
1. Did you see success message after checkout?
2. Did cart get cleared?
3. Check database: Run `SELECT * FROM Orders;`
4. If no records, order didn't save - try checkout again
5. Check for error messages during checkout

---

### Issue: See other user's orders

**Cause:** User isolation not working

**Solution:**
1. This should NOT happen
2. If it does, it's a security bug
3. Check CartController.PaymentHistory() code
4. Should have: `.Where(o => o.UserId == user.Id)`
5. Report if you see this happening

---

## Success Checklist

Use this to verify everything works:

- [ ] Added items to cart
- [ ] Clicked checkout
- [ ] If not logged in: Redirected to login page
- [ ] Logged in successfully
- [ ] Order created (saw success message)
- [ ] Cart cleared
- [ ] Created at least 2 orders
- [ ] Clicked payment history icon
- [ ] Modal opened and loaded (not stuck on "Loading...")
- [ ] Orders table displayed
- [ ] Could see order dates, items, totals, status
- [ ] Clicked order to expand details
- [ ] Saw items with products and quantities
- [ ] Clicked to collapse details
- [ ] Logged out
- [ ] Logged in as different user
- [ ] Payment history showed different orders
- [ ] Did NOT see first user's orders

**If all checked:** ✅ Everything works perfectly!

---

## Next Steps

### If Testing Successful
1. ✅ Checkout system ready for production
2. ✅ Payment history working
3. ✅ Security verified
4. ✅ Ready to deploy

### If Issues Found
1. ❓ Check debugging checklist above
2. ❓ Look at browser console for errors
3. ❓ Check Network tab in F12
4. ❓ Restart application
5. ❓ Clear browser cache (Ctrl+Shift+Del)

---

## Support

### Documentation Files
- **SIMPLE_CHECKOUT_SUMMARY.md** - Complete technical guide
- **ARCHITECTURE_DIAGRAMS.md** - Visual flows and diagrams
- **README_IMPLEMENTATION.md** - Quick reference

### Code Files
- **CartController.cs** - Checkout & PaymentHistory actions
- **_PaymentHistoryPartial.cshtml** - Order history table
- **_Layout.cshtml** - History icon & modal

---

**Ready to test? Start the app and follow the steps above!** 🚀
